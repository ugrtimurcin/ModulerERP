import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { ArrowLeft, Save } from 'lucide-react';
import { Button, Input } from '@/components/ui';
import { api } from '@/lib/api';
import { ProjectStatus } from '@/types/project';
import type { ProjectDto, CreateProjectDto } from '@/types/project';
import { PaymentsTab } from './tabs/PaymentsTab';
import { TasksTab } from './tabs/TasksTab';
import { ResourcesTab } from './tabs/ResourcesTab';
import { DocumentsTab } from './tabs/DocumentsTab';
import { ChangeOrdersTab } from './tabs/ChangeOrdersTab';
import { BoQTab } from './tabs/BoQTab';
import { DailyLogTab } from './tabs/DailyLogTab';
import { RateCardsTab } from './tabs/RateCardsTab';

export function ProjectDetailPage({ mode = 'view' }: { mode?: 'view' | 'create' | 'edit' }) {
    const { id } = useParams();
    const navigate = useNavigate();
    const { t } = useTranslation();
    const [loading, setLoading] = useState(mode !== 'create');
    const [project, setProject] = useState<ProjectDto | null>(null);

    // Tabs State
    const [activeTab, setActiveTab] = useState('details');

    // Form State
    const [formData, setFormData] = useState<Partial<CreateProjectDto>>({
        code: '',
        name: '',
        description: '',
        contractAmount: 0,
        contractCurrencyId: '00000000-0000-0000-0000-000000000000', // Default
        startDate: new Date().toISOString().split('T')[0]
    });

    useEffect(() => {
        if (mode !== 'create' && id) {
            loadProject(id);
        }
    }, [id, mode]);

    const loadProject = async (projectId: string) => {
        try {
            const response = await api.get<{ data: ProjectDto }>(`/projects/${projectId}`);
            if (response.data) {
                setProject(response.data);
                setFormData({
                    code: response.data.code,
                    name: response.data.name,
                    description: response.data.description,
                    contractAmount: response.data.contractAmount,
                    contractCurrencyId: response.data.contractCurrencyId,
                    startDate: response.data.startDate.split('T')[0]
                });
            }
        } catch (error) {
            console.error('Failed to load project', error);
        } finally {
            setLoading(false);
        }
    };

    const handleSave = async () => {
        try {
            if (mode === 'create') {
                const response = await api.post<{ data: ProjectDto }>('/projects', formData);
                if (response.data) {
                    navigate(`/projects/${response.data.id}`);
                }
            } else if ((mode === 'edit' || mode === 'view') && id) {
                // 'view' mode might turn into edit if we allow inline edits or have an edit button
                // For now assuming we are in edit mode if saving
                await api.put(`/projects/${id}`, {
                    name: formData.name!,
                    description: formData.description!,
                    status: project?.status ?? ProjectStatus.Planning
                });
                loadProject(id);
            }
        } catch (error) {
            console.error('Failed to save', error);
        }
    };

    if (loading) return <div>{t('common.loading')}</div>;

    const tabs = [
        { id: 'details', label: t('projects.tabs.details') },
        ...(mode !== 'create' ? [
            { id: 'tasks', label: t('projects.tabs.tasks') },
            { id: 'resources', label: t('projects.tabs.resources') },
            { id: 'boq', label: t('projects.tabs.boq') },
            { id: 'daily-logs', label: t('projects.tabs.dailyLogs') },
            { id: 'payments', label: t('projects.tabs.payments') },
            { id: 'changeOrders', label: t('projects.tabs.changeOrders') },
            { id: 'rateCards', label: t('projects.tabs.rateCards') }, // New
            { id: 'documents', label: t('projects.tabs.documents') }
        ] : [])
    ];

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between">
                <div className="flex items-center space-x-4">
                    <Button variant="ghost" size="icon" onClick={() => navigate('/projects')}>
                        <ArrowLeft className="h-4 w-4" />
                    </Button>
                    <div>
                        <h1 className="text-2xl font-bold tracking-tight">
                            {mode === 'create' ? t('projects.createProject') : project?.name}
                        </h1>
                        <p className="text-sm text-muted-foreground">
                            {mode === 'create' ? t('projects.subtitle') : project?.code}
                        </p>
                    </div>
                </div>
                <div className="flex space-x-2">
                    <Button onClick={handleSave}>
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

            <div className="mt-6">
                {activeTab === 'details' && (
                    <div className="bg-[hsl(var(--card))] rounded-xl border p-6">
                        <h2 className="text-lg font-semibold mb-4">{t('projects.tabs.details')}</h2>
                        <div className="grid gap-4 md:grid-cols-2">
                            <div className="space-y-2">
                                <Input
                                    label={t('projects.code')}
                                    value={formData.code}
                                    onChange={e => setFormData({ ...formData, code: e.target.value })}
                                    disabled={mode !== 'create'}
                                />
                            </div>
                            <div className="space-y-2">
                                <Input
                                    label={t('projects.name')}
                                    value={formData.name}
                                    onChange={e => setFormData({ ...formData, name: e.target.value })}
                                />
                            </div>
                            <div className="space-y-2 md:col-span-2">
                                <Input
                                    label={t('projects.description')}
                                    value={formData.description}
                                    onChange={e => setFormData({ ...formData, description: e.target.value })}
                                />
                            </div>
                            <div className="space-y-2">
                                <Input
                                    label={t('projects.contractAmount')}
                                    type="number"
                                    value={formData.contractAmount}
                                    onChange={e => setFormData({ ...formData, contractAmount: parseFloat(e.target.value) })}
                                    disabled={mode !== 'create'}
                                />
                            </div>
                            <div className="space-y-2">
                                <Input
                                    label={t('projects.startDate')}
                                    type="date"
                                    value={formData.startDate}
                                    onChange={e => setFormData({ ...formData, startDate: e.target.value })}
                                />
                            </div>
                        </div>
                    </div>
                )}

                {activeTab === 'tasks' && id && (
                    <TasksTab projectId={id} />
                )}

                {activeTab === 'resources' && id && (
                    <ResourcesTab projectId={id} />
                )}

                {activeTab === 'boq' && id && (
                    <BoQTab projectId={id} />
                )}

                {activeTab === 'daily-logs' && id && (
                    <DailyLogTab projectId={id} />
                )}

                {activeTab === 'payments' && id && (
                    <PaymentsTab projectId={id} />
                )}

                {activeTab === 'changeOrders' && id && (
                    <ChangeOrdersTab projectId={id} />
                )}

                {activeTab === 'rateCards' && id && (
                    <RateCardsTab projectId={id} />
                )}

                {activeTab === 'documents' && id && (
                    <DocumentsTab projectId={id} />
                )}
            </div>
        </div>
    );
}
