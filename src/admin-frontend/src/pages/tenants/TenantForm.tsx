import { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { tenantService, type CreateTenantRequest, type UpdateTenantRequest } from '../../services/tenantService';
import { Loader2, Save, ArrowLeft } from 'lucide-react';
import { Link } from 'react-router-dom';

export default function TenantForm() {
    const { id } = useParams();
    const navigate = useNavigate();
    const isEditMode = !!id;

    const [isLoading, setIsLoading] = useState(false);
    const [header, setHeader] = useState(isEditMode ? 'Edit Tenant' : 'New Tenant');

    // Form State
    const [formData, setFormData] = useState({
        name: '',
        subdomain: '',
        adminEmail: '',
        adminPassword: '',
        adminFirstName: '',
        adminLastName: '',
        baseCurrencyId: '00000000-0000-0000-0001-000000000002', // Default to seeded USD
        subscriptionPlan: 'Standard',
        isActive: true,
        subscriptionExpiresAt: ''
    });

    useEffect(() => {
        if (isEditMode) {
            loadTenant();
        }
    }, [id]);

    const loadTenant = async () => {
        if (!id) return;
        setIsLoading(true);
        try {
            const tenant = await tenantService.getById(id);
            setFormData({
                ...formData,
                name: tenant.name,
                subdomain: tenant.subdomain,
                baseCurrencyId: tenant.baseCurrencyId,
                subscriptionPlan: tenant.subscriptionPlan || '',
                isActive: tenant.isActive,
                subscriptionExpiresAt: tenant.subscriptionExpiresAt ? tenant.subscriptionExpiresAt.split('T')[0] : ''
            });
            setHeader(`Edit Tenant: ${tenant.name}`);
        } catch (error) {
            console.error('Failed to load tenant', error);
        } finally {
            setIsLoading(false);
        }
    };

    const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
        const { name, value, type } = e.target;
        setFormData(prev => ({
            ...prev,
            [name]: type === 'checkbox' ? (e.target as HTMLInputElement).checked : value
        }));
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setIsLoading(true);

        try {
            if (isEditMode && id) {
                const updateData: UpdateTenantRequest = {
                    name: formData.name,
                    subscriptionPlan: formData.subscriptionPlan,
                    subscriptionExpiresAt: formData.subscriptionExpiresAt ? new Date(formData.subscriptionExpiresAt).toISOString() : undefined,
                    isActive: formData.isActive
                };
                await tenantService.update(id, updateData);
            } else {
                const createData: CreateTenantRequest = {
                    name: formData.name,
                    subdomain: formData.subdomain,
                    adminEmail: formData.adminEmail,
                    adminPassword: formData.adminPassword,
                    adminFirstName: formData.adminFirstName,
                    adminLastName: formData.adminLastName,
                    baseCurrencyId: formData.baseCurrencyId,
                    subscriptionPlan: formData.subscriptionPlan
                };
                await tenantService.create(createData);
            }
            navigate('/tenants');
        } catch (error) {
            console.error('Failed to save tenant', error);
            // TODO: Show error notification
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between">
                <div className="flex items-center space-x-4">
                    <Link to="/tenants" className="text-gray-500 hover:text-gray-700">
                        <ArrowLeft className="h-6 w-6" />
                    </Link>
                    <h1 className="text-2xl font-semibold text-gray-900">{header}</h1>
                </div>
            </div>

            <div className="bg-white shadow rounded-lg p-6">
                <form onSubmit={handleSubmit} className="space-y-6">
                    <div className="grid grid-cols-1 gap-y-6 gap-x-4 sm:grid-cols-6">

                        {/* Tenant Info */}
                        <div className="sm:col-span-3">
                            <label htmlFor="name" className="block text-sm font-medium text-gray-700">Tenant Name</label>
                            <div className="mt-1">
                                <input
                                    type="text"
                                    name="name"
                                    id="name"
                                    required
                                    value={formData.name}
                                    onChange={handleChange}
                                    className="shadow-sm focus:ring-primary focus:border-primary block w-full sm:text-sm border-gray-300 rounded-md p-2 border"
                                />
                            </div>
                        </div>

                        <div className="sm:col-span-3">
                            <label htmlFor="subdomain" className="block text-sm font-medium text-gray-700">Subdomain</label>
                            <div className="mt-1">
                                <input
                                    type="text"
                                    name="subdomain"
                                    id="subdomain"
                                    required
                                    disabled={isEditMode}
                                    value={formData.subdomain}
                                    onChange={handleChange}
                                    className="shadow-sm focus:ring-primary focus:border-primary block w-full sm:text-sm border-gray-300 rounded-md p-2 border disabled:bg-gray-100"
                                />
                            </div>
                        </div>

                        <div className="sm:col-span-3">
                            <label htmlFor="subscriptionPlan" className="block text-sm font-medium text-gray-700">Subscription Plan</label>
                            <div className="mt-1">
                                <select
                                    id="subscriptionPlan"
                                    name="subscriptionPlan"
                                    value={formData.subscriptionPlan}
                                    onChange={handleChange}
                                    className="shadow-sm focus:ring-primary focus:border-primary block w-full sm:text-sm border-gray-300 rounded-md p-2 border"
                                >
                                    <option value="Standard">Standard</option>
                                    <option value="Professional">Professional</option>
                                    <option value="Enterprise">Enterprise</option>
                                </select>
                            </div>
                        </div>

                        {isEditMode && (
                            <div className="sm:col-span-3">
                                <label htmlFor="subscriptionExpiresAt" className="block text-sm font-medium text-gray-700">Expires At</label>
                                <div className="mt-1">
                                    <input
                                        type="date"
                                        name="subscriptionExpiresAt"
                                        id="subscriptionExpiresAt"
                                        value={formData.subscriptionExpiresAt}
                                        onChange={handleChange}
                                        className="shadow-sm focus:ring-primary focus:border-primary block w-full sm:text-sm border-gray-300 rounded-md p-2 border"
                                    />
                                </div>
                            </div>
                        )}

                        <div className="sm:col-span-6">
                            <div className="flex items-start">
                                <div className="flex items-center h-5">
                                    <input
                                        id="isActive"
                                        name="isActive"
                                        type="checkbox"
                                        checked={formData.isActive}
                                        onChange={handleChange}
                                        className="focus:ring-primary h-4 w-4 text-primary border-gray-300 rounded"
                                    />
                                </div>
                                <div className="ml-3 text-sm">
                                    <label htmlFor="isActive" className="font-medium text-gray-700">Active</label>
                                    <p className="text-gray-500">Tenant can access the system</p>
                                </div>
                            </div>
                        </div>

                        {/* Admin User Info - Only for Create */}
                        {!isEditMode && (
                            <>
                                <div className="sm:col-span-6 border-t pt-6 mt-6">
                                    <h3 className="text-lg font-medium leading-6 text-gray-900">Initial Admin User</h3>
                                </div>

                                <div className="sm:col-span-3">
                                    <label htmlFor="adminFirstName" className="block text-sm font-medium text-gray-700">First Name</label>
                                    <div className="mt-1">
                                        <input
                                            type="text"
                                            name="adminFirstName"
                                            id="adminFirstName"
                                            required
                                            value={formData.adminFirstName}
                                            onChange={handleChange}
                                            className="shadow-sm focus:ring-primary focus:border-primary block w-full sm:text-sm border-gray-300 rounded-md p-2 border"
                                        />
                                    </div>
                                </div>

                                <div className="sm:col-span-3">
                                    <label htmlFor="adminLastName" className="block text-sm font-medium text-gray-700">Last Name</label>
                                    <div className="mt-1">
                                        <input
                                            type="text"
                                            name="adminLastName"
                                            id="adminLastName"
                                            required
                                            value={formData.adminLastName}
                                            onChange={handleChange}
                                            className="shadow-sm focus:ring-primary focus:border-primary block w-full sm:text-sm border-gray-300 rounded-md p-2 border"
                                        />
                                    </div>
                                </div>

                                <div className="sm:col-span-3">
                                    <label htmlFor="adminEmail" className="block text-sm font-medium text-gray-700">Email</label>
                                    <div className="mt-1">
                                        <input
                                            type="email"
                                            name="adminEmail"
                                            id="adminEmail"
                                            required
                                            value={formData.adminEmail}
                                            onChange={handleChange}
                                            className="shadow-sm focus:ring-primary focus:border-primary block w-full sm:text-sm border-gray-300 rounded-md p-2 border"
                                        />
                                    </div>
                                </div>

                                <div className="sm:col-span-3">
                                    <label htmlFor="adminPassword" className="block text-sm font-medium text-gray-700">Password</label>
                                    <div className="mt-1">
                                        <input
                                            type="password"
                                            name="adminPassword"
                                            id="adminPassword"
                                            required
                                            value={formData.adminPassword}
                                            onChange={handleChange}
                                            className="shadow-sm focus:ring-primary focus:border-primary block w-full sm:text-sm border-gray-300 rounded-md p-2 border"
                                        />
                                    </div>
                                </div>
                            </>
                        )}
                    </div>

                    <div className="pt-5">
                        <div className="flex justify-end">
                            <Link
                                to="/tenants"
                                className="bg-white py-2 px-4 border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary"
                            >
                                Cancel
                            </Link>
                            <button
                                type="submit"
                                disabled={isLoading}
                                className="ml-3 inline-flex justify-center py-2 px-4 border border-transparent shadow-sm text-sm font-medium rounded-md text-primary-foreground bg-primary hover:bg-primary/90 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary disabled:opacity-50"
                            >
                                {isLoading ? (
                                    <>
                                        <Loader2 className="animate-spin -ml-1 mr-2 h-5 w-5" />
                                        Saving...
                                    </>
                                ) : (
                                    <>
                                        <Save className="-ml-1 mr-2 h-5 w-5" />
                                        Save
                                    </>
                                )}
                            </button>
                        </div>
                    </div>
                </form>
            </div>
        </div>
    );
}
