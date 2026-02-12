import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import type { ProgressPaymentDto } from '@/types/project';
import { projectService } from '@/services/projectService';
import { Button } from '@/components/ui';
import { Plus, Check, FileText } from 'lucide-react';
import { useDialog } from '@/components/ui/Dialog';
import { ProgressPaymentWizard } from '../components/wizard/ProgressPaymentWizard';

interface PaymentsTabProps {
    projectId: string;
}

export function PaymentsTab({ projectId }: PaymentsTabProps) {
    const { t } = useTranslation();
    const [loading, setLoading] = useState(true);
    const [payments, setPayments] = useState<ProgressPaymentDto[]>([]);
    const [isDialogOpen, setIsDialogOpen] = useState(false);
    const [selectedPayment, setSelectedPayment] = useState<ProgressPaymentDto | undefined>(undefined);
    const { confirm } = useDialog();

    useEffect(() => {
        loadPayments();
    }, [projectId]);

    const loadPayments = async () => {
        try {
            const response = await projectService.payments.getByProject(projectId);
            if (response.success && response.data) {
                setPayments(response.data);
            }
        } catch (error) {
            console.error('Failed to load payments', error);
        } finally {
            setLoading(false);
        }
    };

    const handleCreate = () => {
        setSelectedPayment(undefined);
        setIsDialogOpen(true);
    };

    const handleView = (payment: ProgressPaymentDto) => {
        setSelectedPayment(payment);
        setIsDialogOpen(true);
    };

    const handleApprove = async (id: string, e: React.MouseEvent) => {
        e.stopPropagation();
        confirm({
            title: t('common.confirm'),
            message: t('common.thisActionCannotBeUndone'),
            confirmText: t('common.approve'),
            onConfirm: async () => {
                try {
                    await projectService.payments.approve(projectId, id);
                    loadPayments();
                } catch (error) {
                    console.error('Failed to approve', error);
                }
            }
        });
    };

    if (loading) return <div>{t('common.loading')}</div>;

    const formatCurrency = (val: number) => new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(val);

    return (
        <div className="space-y-4">
            <div className="flex justify-between items-center">
                <h3 className="text-lg font-medium">{t('projects.tabs.payments')}</h3>
                <Button onClick={handleCreate}>
                    <Plus className="mr-2 h-4 w-4" />
                    {t('projects.payments.createHakedis')}
                </Button>
            </div>

            <div className="rounded-md border bg-card">
                <table className="w-full text-sm">
                    <thead>
                        <tr className="border-b bg-muted/50">
                            <th className="p-3 text-left">{t('projects.payments.paymentNo')}</th>
                            <th className="p-3 text-left">{t('common.period')}</th>
                            <th className="p-3 text-right">{t('projects.payments.grossAmount')}</th>
                            <th className="p-3 text-right">{t('projects.payments.deductions')}</th>
                            <th className="p-3 text-right">{t('projects.payments.netAmount')}</th>
                            <th className="p-3 text-center">{t('projects.payments.status')}</th>
                            <th className="p-3 text-right">{t('common.actions')}</th>
                        </tr>
                    </thead>
                    <tbody>
                        {payments.length === 0 ? (
                            <tr>
                                <td colSpan={7} className="p-4 text-center text-muted-foreground">
                                    {t('common.noData')}
                                </td>
                            </tr>
                        ) : (
                            payments.map(payment => (
                                <tr
                                    key={payment.id}
                                    className="border-b last:border-0 hover:bg-muted/50 cursor-pointer"
                                    onClick={() => handleView(payment)}
                                >
                                    <td className="p-3 font-medium">#{payment.paymentNo}</td>
                                    <td className="p-3">
                                        {new Date(payment.periodStart).toLocaleDateString()} - {new Date(payment.periodEnd).toLocaleDateString()}
                                    </td>
                                    <td className="p-3 text-right font-medium">{formatCurrency(payment.grossWorkAmount)}</td>
                                    <td className="p-3 text-right text-red-500">
                                        -{formatCurrency(payment.retentionAmount + payment.withholdingTaxAmount + payment.advanceDeductionAmount)}
                                    </td>
                                    <td className="p-3 text-right font-bold text-green-600">{formatCurrency(payment.netPayableAmount)}</td>
                                    <td className="p-3 text-center">
                                        <span className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${payment.status === 0 ? 'bg-yellow-100 text-yellow-800' :
                                            payment.status === 1 ? 'bg-green-100 text-green-800' :
                                                'bg-blue-100 text-blue-800'
                                            }`}>
                                            {payment.status === 0 ? t('projects.payments.statuses.draft') : payment.status === 1 ? t('projects.payments.statuses.approved') : t('projects.payments.statuses.invoiced')}
                                        </span>
                                    </td>
                                    <td className="p-3 text-right">
                                        {payment.status === 0 && (
                                            <Button variant="ghost" size="sm" onClick={(e) => handleApprove(payment.id, e)}>
                                                <Check className="h-4 w-4 mr-1" /> {t('common.approve')}
                                            </Button>
                                        )}
                                        {payment.status === 1 && (
                                            <Button variant="ghost" size="sm" disabled>
                                                <FileText className="h-4 w-4 mr-1" /> {t('projects.payments.invoice')}
                                            </Button>
                                        )}
                                    </td>
                                </tr>
                            ))
                        )}
                    </tbody>
                </table>
            </div>

            <ProgressPaymentWizard
                isOpen={isDialogOpen}
                onClose={() => setIsDialogOpen(false)}
                projectId={projectId}
                payment={selectedPayment}
                onSaved={() => {
                    loadPayments();
                    // Close dialog after save/edit
                    setIsDialogOpen(false);
                }}
            />
        </div>
    );
}
