import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { ArrowLeft, Save } from 'lucide-react';
import { Button, Input } from '@/components/ui';
import { projectService } from '@/services/projectService';
import { ProjectStatus } from '@/types/project';
import type { ProjectDto, CreateProjectDto, UpdateProjectDto } from '@/types/project';

export function ProjectDetailPage({ mode = 'view' }: { mode?: 'view' | 'create' | 'edit' }) {
    const { id } = useParams();
    const navigate = useNavigate();
    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    // const { t } = useTranslation(); // t is unused, removing or commenting out if needed, but better to remove destructuring if purely unused. 
    // Actually typically: const { t } = useTranslation(); -> const { } = ... or just remove if hook not needed?
    // If useTranslation is used for other things?
    // Let's check imports.
    // Assuming just removing t is enough.
    const { i18n } = useTranslation();
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
            const response = await projectService.projects.getById(projectId);
            if (response.success && response.data) {
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
                const response = await projectService.projects.create(formData as CreateProjectDto);
                if (response.success && response.data) {
                    navigate(`/projects/${response.data.id}`);
                }
            } else if (mode === 'edit' && id) {
                await projectService.projects.update(id, {
                    name: formData.name!,
                    description: formData.description!,
                    status: project?.status ?? ProjectStatus.Planning
                } as UpdateProjectDto);
                loadProject(id);
            }
        } catch (error) {
            console.error('Failed to save', error);
        }
    };

    if (loading) return <div>Loading...</div>;

    const tabs = [
        { id: 'details', label: 'Details' },
        ...(mode !== 'create' ? [
            { id: 'tasks', label: 'Tasks (WBS)' },
            { id: 'budget', label: 'Budget & Costs' },
            { id: 'payments', label: 'Progress Payments' },
            { id: 'documents', label: 'Documents' }
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
                            {mode === 'create' ? 'New Project' : project?.name}
                        </h1>
                        <p className="text-sm text-muted-foreground">
                            {mode === 'create' ? 'Create a new project' : project?.code}
                        </p>
                    </div>
                </div>
                <div className="flex space-x-2">
                    <Button onClick={handleSave}>
                        <Save className="mr-2 h-4 w-4" />
                        Save
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
                        <h2 className="text-lg font-semibold mb-4">Project Information</h2>
                        <div className="grid gap-4 md:grid-cols-2">
                            <div className="space-y-2">
                                <Input
                                    label="Code"
                                    value={formData.code}
                                    onChange={e => setFormData({ ...formData, code: e.target.value })}
                                    disabled={mode !== 'create'}
                                />
                            </div>
                            <div className="space-y-2">
                                <Input
                                    label="Name"
                                    value={formData.name}
                                    onChange={e => setFormData({ ...formData, name: e.target.value })}
                                />
                            </div>
                            <div className="space-y-2 md:col-span-2">
                                <Input
                                    label="Description"
                                    value={formData.description}
                                    onChange={e => setFormData({ ...formData, description: e.target.value })}
                                />
                            </div>
                            <div className="space-y-2">
                                <Input
                                    label="Contract Amount"
                                    type="number"
                                    value={formData.contractAmount}
                                    onChange={e => setFormData({ ...formData, contractAmount: parseFloat(e.target.value) })}
                                    disabled={mode !== 'create'}
                                />
                            </div>
                            <div className="space-y-2">
                                <Input
                                    label="Start Date"
                                    type="date"
                                    value={formData.startDate}
                                    onChange={e => setFormData({ ...formData, startDate: e.target.value })}
                                />
                            </div>
                        </div>
                    </div>
                )}

                {activeTab === 'tasks' && (
                    <div className="bg-[hsl(var(--card))] rounded-xl border p-6">
                        <h2 className="text-lg font-semibold mb-4">Work Breakdown Structure</h2>
                        <div className="flex items-center justify-center p-8 text-muted-foreground">
                            Task Tree component will go here.
                        </div>
                    </div>
                )}

                {activeTab === 'budget' && (
                    <div className="bg-[hsl(var(--card))] rounded-xl border p-6">
                        <h2 className="text-lg font-semibold mb-4">Financial Overview</h2>
                        <div className="grid gap-4 md:grid-cols-4">
                            <div className="bg-[hsl(var(--background))] p-4 rounded-lg border">
                                <div className="text-sm font-medium text-muted-foreground">Total Budget</div>
                                <div className="text-2xl font-bold mt-2">{project?.budget.totalBudget.toLocaleString()}</div>
                            </div>
                            {/* More budget cards */}
                        </div>
                    </div>
                )}

                {activeTab === 'payments' && (
                    <div className="bg-[hsl(var(--card))] rounded-xl border p-6">
                        <h2 className="text-lg font-semibold mb-4">Progress Payments (Hakediş)</h2>
                        <div className="flex items-center justify-center p-8 text-muted-foreground">
                            Payment List component will go here.
                        </div>
                    </div>
                )}
            </div>
        </div>
    );
}
