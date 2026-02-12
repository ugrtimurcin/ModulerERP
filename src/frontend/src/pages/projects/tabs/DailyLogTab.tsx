import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus } from 'lucide-react';
import { Button } from '@/components/ui';
import { useToast } from '@/components/ui/Toast';
import { dailyLogService } from '@/services/dailyLogService';
import type { DailyLogDto, CreateDailyLogDto } from '@/types/project';
import { DailyLogDialog } from '../components/DailyLogDialog';
import { DailyLogDataGrid } from '../components/DailyLogDataGrid';

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

            <DailyLogDataGrid logs={logs} onRefresh={loadData} />

            <DailyLogDialog
                isOpen={isDialogOpen}
                onClose={() => setIsDialogOpen(false)}
                onSubmit={handleCreate}
                projectId={projectId}
            />
        </div>
    );
}
