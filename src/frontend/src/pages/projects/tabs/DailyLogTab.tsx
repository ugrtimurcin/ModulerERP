import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Calendar, CheckCircle } from 'lucide-react';
import { Button } from '@/components/ui';
import { useToast } from '@/components/ui/Toast';
import { dailyLogService } from '@/services/dailyLogService';
import type { DailyLogDto, CreateDailyLogDto } from '@/types/project';
import { DailyLogDialog } from '../components/DailyLogDialog';

interface DailyLogTabProps {
    projectId: string;
}

export function DailyLogTab({ projectId }: DailyLogTabProps) {
    const { t } = useTranslation();
    const toast = useToast();
    const [logs, setLogs] = useState<DailyLogDto[]>([]);
    const [loading, setLoading] = useState(true);
    const [isDialogOpen, setIsDialogOpen] = useState(false);

    useEffect(() => {
        loadData();
    }, [projectId]);

    const loadData = async () => {
        setLoading(true);
        try {
            const res = await dailyLogService.getByProject(projectId);
            if (res.success && res.data) {
                setLogs(res.data);
            }
        } catch (error) {
            console.error('Failed to load daily logs', error);
            toast.error(t('common.error'), t('common.errorLoading'));
        } finally {
            setLoading(false);
        }
    };

    const handleCreate = async (data: CreateDailyLogDto) => {
        try {
            const res = await dailyLogService.create(data);
            if (res.success) {
                toast.success(t('common.success'), t('common.saved'));
                setIsDialogOpen(false);
                loadData();
            } else {
                toast.error(t('common.error'), res.error || t('common.error'));
            }
        } catch (error) {
            console.error('Failed to create daily log', error);
            toast.error(t('common.error'), t('common.errorSaving'));
        }
    };

    const handleApprove = async (id: string) => {
        if (!window.confirm(t('projects.dailyLogs.confirmApprove'))) return;

        try {
            const res = await dailyLogService.approve(id);
            if (res.success) {
                toast.success(t('common.success'), t('projects.dailyLogs.approved'));
                loadData();
            } else {
                toast.error(t('common.error'), res.error || t('common.error'));
            }
        } catch (error) {
            console.error('Failed to approve daily log', error);
            toast.error(t('common.error'), t('common.errorSaving'));
        }
    };

    if (loading) return <div className="p-4">{t('common.loading')}</div>;

    return (
        <div className="space-y-4">
            <div className="flex justify-between items-center">
                <h3 className="text-lg font-medium">{t('projects.dailyLogs.title')}</h3>
                <Button onClick={() => setIsDialogOpen(true)}>
                    <Plus className="mr-2 h-4 w-4" />
                    {t('projects.dailyLogs.addDailyLog')}
                </Button>
            </div>

            <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
                {logs.map(log => (
                    <div key={log.id} className="border rounded-xl p-4 bg-card shadow-sm hover:shadow-md transition-all">
                        <div className="flex justify-between items-start mb-2">
                            <div className="flex items-center gap-2 text-sm text-muted-foreground">
                                <Calendar className="h-4 w-4" />
                                {new Date(log.date).toLocaleDateString()}
                            </div>
                            <div className={`px-2 py-1 rounded-full text-xs font-medium ${log.isApproved ? 'bg-green-100 text-green-700' : 'bg-yellow-100 text-yellow-700'}`}>
                                {log.isApproved ? t('common.approved') : t('common.draft')}
                            </div>
                        </div>

                        <div className="mb-3">
                            <p className="font-medium text-sm">{t('projects.dailyLogs.weather')}: {log.weatherCondition}</p>
                            {log.siteManagerNote && (
                                <p className="text-sm text-muted-foreground mt-1 line-clamp-2">{log.siteManagerNote}</p>
                            )}
                        </div>

                        <div className="border-t pt-2 mt-2">
                            <div className="flex justify-between items-center text-sm">
                                <span className="text-muted-foreground">{t('projects.dailyLogs.resources')}: {log.resourceUsages?.length || 0}</span>
                                {!log.isApproved && (
                                    <Button variant="ghost" size="sm" onClick={() => handleApprove(log.id)} className="text-green-600 hover:text-green-700 hover:bg-green-50">
                                        <CheckCircle className="h-4 w-4 mr-1" />
                                        {t('common.approve')}
                                    </Button>
                                )}
                            </div>
                        </div>
                    </div>
                ))}

                {logs.length === 0 && (
                    <div className="col-span-full text-center py-10 text-muted-foreground">
                        {t('common.noData')}
                    </div>
                )}
            </div>

            <DailyLogDialog
                isOpen={isDialogOpen}
                onClose={() => setIsDialogOpen(false)}
                onSubmit={handleCreate}
                projectId={projectId}
            />
        </div>
    );
}
