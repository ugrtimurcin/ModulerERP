import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { Button } from '@/components/ui';
import { Plus, Check, X } from 'lucide-react';
import { useDialog } from '@/components/ui/Dialog';
import { projectService } from '@/services/projectService';
import { ChangeOrderDialog } from '../components/ChangeOrderDialog';
import type { ProjectChangeOrderDto } from '@/types/project';

interface ChangeOrdersTabProps {
    projectId: string;
}

export function ChangeOrdersTab({ projectId }: ChangeOrdersTabProps) {
    const { t } = useTranslation();
    const [loading, setLoading] = useState(true);
    const [changeOrders, setChangeOrders] = useState<ProjectChangeOrderDto[]>([]);
    const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
    const { confirm, danger } = useDialog();

    useEffect(() => {
        loadChangeOrders();
    }, [projectId]);

    const loadChangeOrders = async () => {
        try {
            setLoading(true);
            const response = await projectService.changeOrders.getByProject(projectId);
            if (response.success && response.data) {
                setChangeOrders(response.data);
            }
        } catch (error) {
            console.error('Failed to load change orders', error);
        } finally {
            setLoading(false);
        }
    };

    const handleApprove = async (id: string) => {
        confirm({
            title: t('common.confirm'),
            message: t('projects.changeOrders.approveConfirmation'),
            confirmText: t('common.approve'),
            onConfirm: async () => {
                try {
                    await projectService.changeOrders.approve(id);
                    loadChangeOrders();
                } catch (error) {
                    console.error('Failed to approve', error);
                }
            }
        });
    };

    const handleReject = async (id: string) => {
        danger({
            title: t('common.reject'),
            message: t('projects.changeOrders.rejectConfirmation'),
            confirmText: t('common.reject'),
            onConfirm: async () => {
                try {
                    await projectService.changeOrders.reject(id);
                    loadChangeOrders();
                } catch (error) {
                    console.error('Failed to reject', error);
                }
            }
        });
    };

    if (loading) return <div>{t('common.loading')}</div>;

    const formatCurrency = (val: number) => new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(val);

    return (
        <div className="space-y-4">
            <div className="flex justify-between items-center">
                <h3 className="text-lg font-medium">{t('projects.tabs.changeOrders')}</h3>
                <Button onClick={() => setIsCreateModalOpen(true)}>
                    <Plus className="mr-2 h-4 w-4" />
                    {t('projects.changeOrders.create')}
                </Button>
            </div>

            <div className="rounded-md border bg-card">
                <table className="w-full text-sm">
                    <thead>
                        <tr className="border-b bg-muted/50">
                            <th className="p-3 text-left">#</th>
                            <th className="p-3 text-left">{t('projects.changeOrders.title')}</th>
                            <th className="p-3 text-right">{t('projects.changeOrders.amountChange')}</th>
                            <th className="p-3 text-right">{t('projects.changeOrders.timeExtension')}</th>
                            <th className="p-3 text-center">{t('common.status')}</th>
                            <th className="p-3 text-right">{t('common.actions')}</th>
                        </tr>
                    </thead>
                    <tbody>
                        {changeOrders.length === 0 ? (
                            <tr>
                                <td colSpan={6} className="p-4 text-center text-muted-foreground">
                                    {t('common.noData')}
                                </td>
                            </tr>
                        ) : (
                            changeOrders.map(order => (
                                <tr key={order.id} className="border-b last:border-0 hover:bg-muted/50">
                                    <td className="p-3 font-medium">#{order.orderNo}</td>
                                    <td className="p-3">
                                        <div className="font-medium">{order.title}</div>
                                        <div className="text-xs text-muted-foreground truncate max-w-[200px]">{order.description}</div>
                                    </td>
                                    <td className={`p-3 text-right font-medium ${order.amountChange > 0 ? 'text-green-600' : order.amountChange < 0 ? 'text-red-500' : ''}`}>
                                        {formatCurrency(order.amountChange)}
                                    </td>
                                    <td className="p-3 text-right">
                                        {order.timeExtensionDays > 0 ? `+${order.timeExtensionDays} days` : '-'}
                                    </td>
                                    <td className="p-3 text-center">
                                        <Badge status={order.status} />
                                    </td>
                                    <td className="p-3 text-right">
                                        {order.status === 0 || order.status === 1 ? ( // Draft or Pending
                                            <div className="flex justify-end space-x-1">
                                                <Button variant="ghost" size="sm" onClick={() => handleApprove(order.id)} title={t('common.approve')}>
                                                    <Check className="h-4 w-4 text-green-600" />
                                                </Button>
                                                <Button variant="ghost" size="sm" onClick={() => handleReject(order.id)} title={t('common.reject')}>
                                                    <X className="h-4 w-4 text-red-600" />
                                                </Button>
                                            </div>
                                        ) : (
                                            <span className="text-muted-foreground text-xs">
                                                {new Date(order.approvalDate).toLocaleDateString()}
                                            </span>
                                        )}
                                    </td>
                                </tr>
                            ))
                        )}
                    </tbody>
                </table>
            </div>

            <ChangeOrderDialog
                isOpen={isCreateModalOpen}
                onClose={() => setIsCreateModalOpen(false)}
                projectId={projectId}
                onSuccess={loadChangeOrders}
            />
        </div>
    );
}

function Badge({ status }: { status: number }) {
    const styles = [
        "bg-gray-100 text-gray-800", // Draft
        "bg-yellow-100 text-yellow-800", // Pending
        "bg-green-100 text-green-800", // Approved
        "bg-red-100 text-red-800" // Rejected
    ];
    const labels = ["Draft", "Pending", "Approved", "Rejected"]; // Should use translation

    return (
        <span className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${styles[status]}`}>
            {labels[status]}
        </span>
    );
}
