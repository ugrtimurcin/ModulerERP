import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Search, Calendar, Folder } from 'lucide-react';
import { Button, Input, Badge } from '@/components/ui';
import { api } from '@/lib/api';
import { ProjectStatus } from '@/types/project';
import type { ProjectDto } from '@/types/project';
import { useNavigate } from 'react-router-dom';

export function ProjectsPage() {
    const { t } = useTranslation();
    const navigate = useNavigate();
    const [projects, setProjects] = useState<ProjectDto[]>([]);
    const [loading, setLoading] = useState(true);
    const [searchTerm, setSearchTerm] = useState('');

    useEffect(() => {
        loadProjects();
    }, []);

    const loadProjects = async () => {
        try {
            const response = await api.get<{ data: ProjectDto[] }>('/projects');
            setProjects(response.data || []);
        } catch (error) {
            console.error('Failed to load projects', error);
        } finally {
            setLoading(false);
        }
    };

    const filteredProjects = projects.filter(p =>
        p.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
        p.code.toLowerCase().includes(searchTerm.toLowerCase())
    );

    const getStatusVariant = (status: ProjectStatus): 'default' | 'success' | 'warning' | 'error' | 'info' => {
        switch (status) {
            case ProjectStatus.Active: return 'success';
            case ProjectStatus.Planning: return 'info';
            case ProjectStatus.Completed: return 'default';
            case ProjectStatus.Suspended: return 'warning';
            case ProjectStatus.Cancelled: return 'error';
            default: return 'default';
        }
    };

    const getStatusText = (status: ProjectStatus) => {
        return Object.keys(ProjectStatus).find(key => ProjectStatus[key as keyof typeof ProjectStatus] === status) || 'Unknown';
    };

    return (
        <div className="space-y-6">
            <div className="flex justify-between items-center">
                <div>
                    <h1 className="text-3xl font-bold tracking-tight">{t('projects.title', 'Projects')}</h1>
                    <p className="text-muted-foreground">{t('projects.subtitle', 'Manage construction projects and tasks')}</p>
                </div>
                <Button onClick={() => navigate('/projects/new')}>
                    <Plus className="mr-2 h-4 w-4" />
                    {t('common.create', 'Create Project')}
                </Button>
            </div>

            <div className="flex items-center space-x-2">
                <div className="relative flex-1 max-w-sm">
                    <Input
                        placeholder={t('common.search', 'Search...')}
                        className="pl-8"
                        value={searchTerm}
                        onChange={(e) => setSearchTerm(e.target.value)}
                    />
                    <Search className="absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground" />
                </div>
            </div>

            {loading ? (
                <div>Loading...</div>
            ) : (
                <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
                    {filteredProjects.map((project) => (
                        <div
                            key={project.id}
                            className="bg-[hsl(var(--card))] rounded-xl border p-5 cursor-pointer hover:shadow-md transition-shadow"
                            onClick={() => navigate(`/projects/${project.id}`)}
                        >
                            <div className="flex flex-row items-center justify-between space-y-0 pb-2 mb-4">
                                <div className="text-sm font-medium">
                                    {project.code}
                                </div>
                                <Badge variant={getStatusVariant(project.status)}>
                                    {getStatusText(project.status)}
                                </Badge>
                            </div>
                            <div>
                                <div className="flex items-center space-x-4 mb-4">
                                    <div className="p-2 bg-primary/10 rounded-full">
                                        <Folder className="h-6 w-6 text-primary" />
                                    </div>
                                    <div>
                                        <div className="text-xl font-bold">{project.name}</div>
                                        <div className="text-xs text-muted-foreground">{project.description}</div>
                                    </div>
                                </div>

                                <div className="space-y-2 text-sm">
                                    <div className="flex items-center text-muted-foreground">
                                        <Calendar className="mr-2 h-4 w-4" />
                                        <span>Start: {new Date(project.startDate).toLocaleDateString()}</span>
                                    </div>
                                    <div className="flex justify-between items-center mt-4">
                                        <span className="text-muted-foreground">Progress</span>
                                        <span className="font-bold">{project.completionPercentage}%</span>
                                    </div>
                                    <div className="w-full bg-secondary h-2 rounded-full overflow-hidden">
                                        <div
                                            className="bg-primary h-full"
                                            style={{ width: `${project.completionPercentage}%` }}
                                        />
                                    </div>
                                </div>
                            </div>
                        </div>
                    ))}
                </div>
            )}
        </div>
    );
}
