import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { ArrowLeft, Save, Building2, Phone } from 'lucide-react';
import { Button, Input, Select, useToast } from '@/components/ui';
import { api } from '@/services/api';
import { ActivitiesTimeline } from '@/components/crm/ActivitiesTimeline';

export function BusinessPartnerDetail({ mode = 'view' }: { mode?: 'view' | 'create' | 'edit' }) {
    const { id } = useParams();
    const navigate = useNavigate();
    const { t } = useTranslation();
    const toast = useToast();

    const [loading, setLoading] = useState(mode !== 'create');
    const [isSubmitting, setIsSubmitting] = useState(false);

    // Dropdown Data
    const [territories, setTerritories] = useState<any[]>([]);
    const [currencies, setCurrencies] = useState<any[]>([]);
    const [partnerGroups, setPartnerGroups] = useState<any[]>([]);

    // Tabs State
    const [activeTab, setActiveTab] = useState('overview');

    // Form State
    const [formData, setFormData] = useState<any>({
        code: '',
        name: '',
        taxNumber: '',
        taxOffice: '',
        email: '',
        phone: '',
        website: '',
        address: '',
        groupId: '',
        customerGroup: '',
        supplierGroup: '',
        isCustomer: true,
        isSupplier: false,
        isActive: true,
        territoryId: '', // Phase 3 Field
        currencyId: '', // Default Currency Phase 3 Field
    });

    useEffect(() => {
        loadDropdowns();
        if (mode !== 'create' && id) {
            loadPartner(id);
        }
    }, [id, mode]);

    const loadDropdowns = async () => {
        try {
            const [tResult, cResult, pgResult] = await Promise.all([
                api.territories.getAll(),
                api.getActiveCurrencies(),
                api.partnerGroups.getAll()
            ]);

            if (tResult.success && tResult.data?.data) {
                setTerritories(tResult.data.data);
            }
            if (cResult.success && cResult.data) {
                setCurrencies(cResult.data);
            }
            if (pgResult.success && pgResult.data?.data) {
                setPartnerGroups(pgResult.data.data);
            }
        } catch (error) {
            console.error('Failed to load dropdowns', error);
        }
    };

    const loadPartner = async (partnerId: string) => {
        try {
            // Simulated fetch since getById might not exist yet
            const response = await api.partners.getAll(1, 100);
            const partner = response.data?.data.find((p: any) => p.id === partnerId) as any;
            if (partner) {
                setFormData({
                    code: partner.code || '',
                    name: partner.name || '',
                    taxNumber: partner.taxNumber || '',
                    taxOffice: partner.taxOffice || '',
                    email: partner.email || '',
                    phone: partner.phone || '',
                    website: partner.website || '',
                    address: partner.address || '',
                    groupId: partner.groupId || '',
                    customerGroup: partner.customerGroup || '',
                    supplierGroup: partner.supplierGroup || '',
                    isCustomer: partner.isCustomer ?? true,
                    isSupplier: partner.isSupplier ?? false,
                    isActive: partner.isActive ?? true,
                    territoryId: partner.territoryId || '',
                    currencyId: partner.currencyId || '',
                });
            }
        } catch (error: any) {
            toast.error(t('common.error'), error.message);
        } finally {
            setLoading(false);
        }
    };

    const handleSave = async () => {
        setIsSubmitting(true);
        try {
            if (mode === 'create') {
                const response = await api.partners.create(formData);
                if (response.success) {
                    toast.success(t('partners.partnerCreated'));
                    navigate('/partners');
                } else {
                    toast.error(t('common.error'), response.error);
                }
            } else if (id) {
                const response = await api.partners.update(id, formData);
                if (response.success) {
                    toast.success(t('partners.partnerUpdated'));
                    loadPartner(id);
                } else {
                    toast.error(t('common.error'), response.error);
                }
            }
        } catch (error: any) {
            toast.error(t('common.error'), error.message);
        } finally {
            setIsSubmitting(false);
        }
    };

    if (loading) return <div className="p-8 text-center text-muted-foreground">{t('common.loading')}</div>;

    const tabs = [
        { id: 'overview', label: t('common.overview') },
        ...(mode !== 'create' ? [
            { id: 'addresses', label: t('partners.addresses', 'Addresses') },
            { id: 'opportunities', label: t('crm.opportunities') },
            { id: 'activities', label: t('activities.timeline') }
        ] : [])
    ];

    return (
        <div className="space-y-6">
            {/* Header */}
            <div className="flex items-center justify-between">
                <div className="flex items-center space-x-4">
                    <Button variant="ghost" size="icon" onClick={() => navigate('/partners')}>
                        <ArrowLeft className="h-4 w-4" />
                    </Button>
                    <div>
                        <h1 className="text-2xl font-bold tracking-tight">
                            {mode === 'create' ? t('partners.createPartner') : formData.name}
                        </h1>
                        <p className="text-sm text-muted-foreground">
                            {mode === 'create' ? t('partners.subtitle') : (formData.code || t('partners.role'))}
                        </p>
                    </div>
                </div>
                <div className="flex space-x-2">
                    <Button onClick={handleSave} isLoading={isSubmitting}>
                        <Save className="mr-2 h-4 w-4" />
                        {t('common.save')}
                    </Button>
                </div>
            </div>

            {/* Tabs List */}
            <div className="border-b">
                <div className="flex space-x-4">
                    {tabs.map(tab => (
                        <button
                            key={tab.id}
                            className={`py-2 px-4 text-sm font-medium border-b-2 transition-colors ${activeTab === tab.id ? 'border-primary text-primary' : 'border-transparent text-muted-foreground hover:text-foreground'}`}
                            onClick={() => setActiveTab(tab.id)}
                        >
                            {tab.label}
                        </button>
                    ))}
                </div>
            </div>

            {/* Tab Contents */}
            <div className="mt-6">
                {activeTab === 'overview' && (
                    <div className="bg-[hsl(var(--card))] rounded-xl border p-6">
                        <div className="grid gap-6 md:grid-cols-2">
                            {/* Company Info */}
                            <div className="space-y-4">
                                <h3 className="font-semibold text-lg flex items-center gap-2">
                                    <Building2 className="w-5 h-5 text-indigo-500" />
                                    {t('partners.companyInfo', 'Company Information')}
                                </h3>
                                <Input
                                    label={t('partners.code')}
                                    value={formData.code}
                                    onChange={e => setFormData({ ...formData, code: e.target.value })}
                                    required
                                />
                                <Input
                                    label={t('partners.name')}
                                    value={formData.name}
                                    onChange={e => setFormData({ ...formData, name: e.target.value })}
                                    required
                                />
                                <div className="grid grid-cols-2 gap-4">
                                    <Input
                                        label={t('partners.taxOffice')}
                                        value={formData.taxOffice}
                                        onChange={e => setFormData({ ...formData, taxOffice: e.target.value })}
                                    />
                                    <Input
                                        label={t('partners.taxNumber')}
                                        value={formData.taxNumber}
                                        onChange={e => setFormData({ ...formData, taxNumber: e.target.value })}
                                    />
                                </div>
                            </div>

                            {/* Contact Info */}
                            <div className="space-y-4">
                                <h3 className="font-semibold text-lg flex items-center gap-2">
                                    <Phone className="w-5 h-5 text-green-500" />
                                    {t('partners.contactInfo', 'Contact Information')}
                                </h3>
                                <Input
                                    label={t('partners.email')}
                                    type="email"
                                    value={formData.email}
                                    onChange={e => setFormData({ ...formData, email: e.target.value })}
                                />
                                <Input
                                    label={t('partners.phone')}
                                    value={formData.phone}
                                    onChange={e => setFormData({ ...formData, phone: e.target.value })}
                                />
                                <Input
                                    label={t('partners.website')}
                                    value={formData.website}
                                    onChange={e => setFormData({ ...formData, website: e.target.value })}
                                    placeholder="https://"
                                />
                                <Input
                                    label={t('partners.address')}
                                    value={formData.address}
                                    onChange={e => setFormData({ ...formData, address: e.target.value })}
                                />
                            </div>

                            {/* Partner Details */}
                            <div className="space-y-4 md:col-span-2 border-t pt-4">
                                <h3 className="font-semibold text-lg">
                                    {t('partners.partnerDetails', 'Partner Details')}
                                </h3>
                                <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
                                    <label className="flex items-center gap-3 cursor-pointer p-3 border rounded-lg hover:bg-slate-50 dark:hover:bg-slate-800 transition-colors">
                                        <input
                                            type="checkbox"
                                            checked={formData.isCustomer}
                                            onChange={(e) => setFormData({ ...formData, isCustomer: e.target.checked })}
                                            className="w-5 h-5 rounded border-gray-300 text-indigo-600 focus:ring-indigo-500"
                                        />
                                        <span className="font-medium text-sm">{t('partners.isCustomer', 'Is Customer')}</span>
                                    </label>
                                    <label className="flex items-center gap-3 cursor-pointer p-3 border rounded-lg hover:bg-slate-50 dark:hover:bg-slate-800 transition-colors">
                                        <input
                                            type="checkbox"
                                            checked={formData.isSupplier}
                                            onChange={(e) => setFormData({ ...formData, isSupplier: e.target.checked })}
                                            className="w-5 h-5 rounded border-gray-300 text-indigo-600 focus:ring-indigo-500"
                                        />
                                        <span className="font-medium text-sm">{t('partners.isSupplier', 'Is Supplier')}</span>
                                    </label>
                                    <Select
                                        label={t('partners.partnerGroup', 'Partner Group')}
                                        value={formData.groupId}
                                        onChange={e => setFormData({ ...formData, groupId: e.target.value })}
                                        options={[
                                            { value: '', label: t('common.none') },
                                            ...partnerGroups.map(pg => ({ value: pg.id, label: pg.name }))
                                        ]}
                                    />
                                    <Select
                                        label={t('crm.territory', 'Territory')}
                                        value={formData.territoryId}
                                        onChange={e => setFormData({ ...formData, territoryId: e.target.value })}
                                        options={[
                                            { value: '', label: t('common.none') },
                                            ...territories.map(t => ({ value: t.id, label: t.name }))
                                        ]}
                                    />
                                    <Select
                                        label={t('crm.currency', 'Default Currency')}
                                        value={formData.currencyId}
                                        onChange={e => setFormData({ ...formData, currencyId: e.target.value })}
                                        options={[
                                            { value: '', label: t('common.none') },
                                            ...currencies.map(c => ({ value: c.id, label: c.code }))
                                        ]}
                                    />
                                </div>
                            </div>
                        </div>
                    </div>
                )}

                {activeTab === 'addresses' && id && (
                    <div className="bg-[hsl(var(--card))] rounded-xl border p-6">
                        <div className="flex justify-between items-center mb-4">
                            <h3 className="text-lg font-semibold">{t('partners.addresses', 'Addresses')}</h3>
                            <Button variant="outline" size="sm">{t('common.add')}</Button>
                        </div>
                        <div className="text-center py-8 text-muted-foreground border-2 border-dashed rounded-lg">
                            {t('common.noData')}
                        </div>
                    </div>
                )}

                {activeTab === 'opportunities' && id && (
                    <div className="bg-[hsl(var(--card))] rounded-xl border p-6">
                        <div className="flex justify-between items-center mb-4">
                            <h3 className="text-lg font-semibold">{t('crm.opportunities', 'Opportunities')}</h3>
                        </div>
                        <div className="text-center py-8 text-muted-foreground border-2 border-dashed rounded-lg">
                            {t('common.noData')}
                        </div>
                    </div>
                )}

                {activeTab === 'activities' && id && (
                    <div className="bg-[hsl(var(--card))] rounded-xl border p-6">
                        <ActivitiesTimeline entityId={id} entityType="BusinessPartner" />
                    </div>
                )}
            </div>
        </div>
    );
}

export default BusinessPartnerDetail;
