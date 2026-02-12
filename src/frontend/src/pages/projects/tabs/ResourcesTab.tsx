import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Trash, User, Truck } from 'lucide-react';
import { Button, Input } from '@/components/ui';
import { MultiSelect } from '@/components/ui/MultiSelect';
import { Modal } from '@/components/ui/Modal';
import { useDialog } from '@/components/ui/Dialog';
import { useToast } from '@/components/ui/Toast';
import { api } from '@/lib/api';
import type { ProjectResourceDto, CreateProjectResourceDto } from '@/types/project';

interface ResourcesTabProps {
    projectId: string;
}

// Extended state for multi-select
interface ResourceFormState extends Omit<CreateProjectResourceDto, 'employeeId' | 'assetId'> {
    employeeIds: string[];
    assetIds: string[];
}

export function ResourcesTab({ projectId }: ResourcesTabProps) {
    const { t } = useTranslation();
    const { confirm } = useDialog();
    const toast = useToast();
    const [loading, setLoading] = useState(true);
    const [resources, setResources] = useState<ProjectResourceDto[]>([]);
    const [isModalOpen, setIsModalOpen] = useState(false);

    // lookup state
    const [resourceType, setResourceType] = useState<'employee' | 'asset' | 'other'>('other');
    const [employees, setEmployees] = useState<{ id: string; firstName: string; lastName: string; position: string }[]>([]);
    const [assets, setAssets] = useState<{ id: string; name: string; serialNumber: string }[]>([]);
    const [loadingLookups, setLoadingLookups] = useState(false);

    // Form State
    const [formData, setFormData] = useState<ResourceFormState>({
        projectId,
        role: '',
        hourlyCost: 0,
        currencyId: '00000000-0000-0000-0000-000000000000',
        employeeIds: [],
        assetIds: []
    });

    useEffect(() => {
        loadResources();
    }, [projectId]);

    useEffect(() => {
        if (isModalOpen) {
            loadLookups();
        }
    }, [isModalOpen]);

    const loadResources = async () => {
        try {
            const response = await api.get<{ data: ProjectResourceDto[] }>(`/projects/${projectId}/resources`);
            if (response.data) {
                setResources(response.data);
            }
        } catch (error) {
            console.error('Failed to load resources', error);
        } finally {
            setLoading(false);
        }
    };

    const loadLookups = async () => {
        setLoadingLookups(true);
        try {
            // Parallel fetch using optimized lookup endpoints
            const [empRes, assetRes] = await Promise.all([
                api.get<{ data: any[] }>('/hr/employees/lookup'),
                api.get<{ data: any[] }>('/fixedassets/assets/lookup')
            ]);

            if (empRes.data) {
                // Lookup returns a direct array, not PagedResult
                setEmployees(empRes.data.map((e: any) => ({
                    id: e.id,
                    firstName: e.firstName,
                    lastName: e.lastName,
                    position: e.Position // Note: Capital P based on Controller anonymous type
                })));
            }
            if (assetRes.data) {
                setAssets(assetRes.data.map((a: any) => ({
                    id: a.id,
                    name: a.name,
                    serialNumber: a.serialNumber
                })));
            }
        } catch (error) {
            console.error("Failed to load lookups", error);
            toast.error(t('common.error'), "Failed to load employees/assets");
        } finally {
            setLoadingLookups(false);
        }
    };

    const handleAddClick = () => {
        setFormData({
            projectId,
            role: '',
            hourlyCost: 0,
            currencyId: '00000000-0000-0000-0000-000000000000',
            employeeIds: [],
            assetIds: []
        });
        setResourceType('other');
        setIsModalOpen(true);
    };

    const handleDeleteClick = (resource: ProjectResourceDto) => {
        confirm({
            title: t('common.delete'),
            message: t('common.thisActionCannotBeUndone'),
            confirmText: t('common.delete'),
            onConfirm: async () => {
                try {
                    await api.delete(`/projectresources/${resource.id}`);
                    toast.success(t('common.success'), t('common.deleted'));
                    loadResources();
                } catch (error) {
                    console.error('Failed to delete resource', error);
                    toast.error(t('common.error'), t('common.errorDeleting'));
                }
            }
        });
    };

    const handleSubmit = async () => {
        try {
            if (!formData.role || formData.hourlyCost < 0) return;

            await api.post('/projectresources', {
                ...formData,
                projectId // Ensure projectId
            });

            // Assuming successful if no error thrown
            toast.success(t('common.success'), t('common.saved'));
            setIsModalOpen(false);
            loadResources();

        } catch (error) {
            console.error('Failed to save resource', error);
            toast.error(t('common.error'), t('common.errorSaving'));
        }
    };

    const handleTypeChange = (value: string) => {
        setResourceType(value as any);
        // Reset specific IDs
        setFormData(prev => ({ ...prev, employeeId: undefined, assetId: undefined, role: '' }));
    };

    const handleEmployeeSelect = (ids: string[]) => {
        // If selecting single employee for first time, auto-fill role
        let newRole = formData.role;
        if (ids.length > 0 && formData.employeeIds.length === 0) {
            const lastId = ids[ids.length - 1]; // although multi, usually we pick one first
            const emp = employees.find(x => x.id === lastId);
            if (emp?.position) newRole = emp.position;
        }

        setFormData(prev => ({
            ...prev,
            employeeIds: ids,
            assetIds: [],
            role: newRole
        }));
    };

    const handleAssetSelect = (ids: string[]) => {
        let newRole = formData.role;
        if (ids.length > 0 && formData.assetIds.length === 0) {
            const lastId = ids[ids.length - 1];
            const asset = assets.find(x => x.id === lastId);
            if (asset?.name) newRole = asset.name;
        }

        setFormData(prev => ({
            ...prev,
            assetIds: ids,
            employeeIds: [],
            role: newRole
        }));
    };

    if (loading) return <div>{t('common.loading')}</div>;

    return (
        <div className="space-y-4">
            <div className="flex justify-between items-center">
                <h3 className="text-lg font-medium">{t('projects.tabs.resources')}</h3>
                <Button onClick={handleAddClick}>
                    <Plus className="mr-2 h-4 w-4" />
                    {t('projects.addResource')}
                </Button>
            </div>

            <div className="rounded-md border bg-card">
                <div className="p-4">
                    {resources.length === 0 ? (
                        <div className="text-center text-muted-foreground py-8">
                            {t('common.noData')}
                        </div>
                    ) : (
                        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
                            {resources.map(resource => (
                                <div key={resource.id} className="flex items-center justify-between p-3 border rounded-lg bg-card hover:bg-accent/5 transition-colors">
                                    <div className="flex items-center gap-3">
                                        <div className="h-10 w-10 rounded-full bg-primary/10 flex items-center justify-center text-primary">
                                            {resource.assetId ? <Truck className="h-5 w-5" /> : <User className="h-5 w-5" />}
                                        </div>
                                        <div>
                                            <p className="font-medium">{resource.role}</p>
                                            <p className="text-xs text-muted-foreground">
                                                {resource.employeeName || resource.assetName || t('common.unassigned')}
                                            </p>
                                        </div>
                                    </div>
                                    <div className="flex items-center gap-2">
                                        <div className="text-sm font-medium">
                                            {resource.hourlyCost} {t('common.currency')}
                                        </div>
                                        <Button variant="ghost" size="icon" className="h-8 w-8 text-red-500 hover:text-red-600" onClick={() => handleDeleteClick(resource)}>
                                            <Trash className="h-4 w-4" />
                                        </Button>
                                    </div>
                                </div>
                            ))}
                        </div>
                    )}
                </div>
            </div>

            <Modal
                isOpen={isModalOpen}
                onClose={() => setIsModalOpen(false)}
                title={t('projects.addResource')}
            >
                <div className="space-y-4 py-4">
                    {/* Resource Type Toggle */}
                    <div className="flex space-x-4 mb-4">
                        <label className="flex items-center space-x-2">
                            <input type="radio" name="resourceType" value="other" checked={resourceType === 'other'} onChange={(e) => handleTypeChange(e.target.value)} />
                            <span>{t('projects.tabs.resource.types.other')}</span>
                        </label>
                        <label className="flex items-center space-x-2">
                            <input type="radio" name="resourceType" value="employee" checked={resourceType === 'employee'} onChange={(e) => handleTypeChange(e.target.value)} />
                            <span>{t('projects.tabs.resource.types.employee')}</span>
                        </label>
                        <label className="flex items-center space-x-2">
                            <input type="radio" name="resourceType" value="asset" checked={resourceType === 'asset'} onChange={(e) => handleTypeChange(e.target.value)} />
                            <span>{t('projects.tabs.resource.types.asset')}</span>
                        </label>
                    </div>

                    {loadingLookups ? (
                        <div className="text-sm text-muted-foreground">{t('common.loading')}</div>
                    ) : (
                        <>
                            {resourceType === 'employee' && (
                                <div className="space-y-2">
                                    <MultiSelect
                                        label={t('projects.tabs.resource.selectEmployees')}
                                        value={formData.employeeIds}
                                        onChange={handleEmployeeSelect}
                                        options={employees.map(e => ({ label: `${e.firstName} ${e.lastName} (${e.position})`, value: e.id }))}
                                        placeholder={t('projects.tabs.resource.selectEmployees') + '...'}
                                    />
                                </div>
                            )}

                            {resourceType === 'asset' && (
                                <div className="space-y-2">
                                    <MultiSelect
                                        label={t('projects.tabs.resource.selectAssets')}
                                        value={formData.assetIds}
                                        onChange={handleAssetSelect}
                                        options={assets.map(a => ({ label: `${a.name} (${a.serialNumber || 'No SN'})`, value: a.id }))}
                                        placeholder={t('projects.tabs.resource.selectAssets') + '...'}
                                    />
                                </div>
                            )}
                        </>
                    )}

                    <Input
                        label={t('projects.role')}
                        value={formData.role}
                        onChange={(e) => setFormData({ ...formData, role: e.target.value })}
                        placeholder="e.g. Foreman, Excavator Operator"
                        required
                    />

                    <div className="grid grid-cols-2 gap-4">
                        <Input
                            label={t('projects.hourlyCost')}
                            type="number"
                            value={formData.hourlyCost}
                            onChange={(e) => setFormData({ ...formData, hourlyCost: parseFloat(e.target.value) })}
                            required
                        />
                        {/* Currency Select would go here */}
                    </div>

                    <div className="flex justify-end space-x-2 mt-4">
                        <Button variant="ghost" onClick={() => setIsModalOpen(false)}>{t('common.cancel')}</Button>
                        <Button onClick={handleSubmit}>{t('common.save')}</Button>
                    </div>
                </div>
            </Modal>
        </div>
    );
}
