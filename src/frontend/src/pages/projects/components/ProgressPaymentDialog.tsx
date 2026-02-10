import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { Modal } from '@/components/ui/Modal';
import { Button, Input } from '@/components/ui';
import { projectService } from '@/services/projectService';
import {
    type ProgressPaymentDto,
    type CreateProgressPaymentDto,
    type ProgressPaymentDetailDto,
    ProgressPaymentStatus
} from '@/types/project';
import { Check, Loader2 } from 'lucide-react';

interface ProgressPaymentDialogProps {
    isOpen: boolean;
    onClose: () => void;
    projectId: string;
    payment?: ProgressPaymentDto; // If provided, edit/view mode. If null, create mode.
    onSaved: () => void;
}

export function ProgressPaymentDialog({ isOpen, onClose, projectId, payment, onSaved }: ProgressPaymentDialogProps) {
    const { t } = useTranslation();
    const [loading, setLoading] = useState(false);
    const [createForm, setCreateForm] = useState<Partial<CreateProgressPaymentDto>>({
        date: new Date().toISOString().split('T')[0],
        periodStart: new Date().toISOString().split('T')[0],
        periodEnd: new Date().toISOString().split('T')[0],
        materialOnSiteAmount: 0,
        advanceDeductionAmount: 0,
        isExpense: false
    });

    // For Edit Mode (Details)
    const [details, setDetails] = useState<ProgressPaymentDetailDto[]>([]);
    const [editingDetailId, setEditingDetailId] = useState<string | null>(null);
    const [tempQuantity, setTempQuantity] = useState<number>(0);

    useEffect(() => {
        if (isOpen && payment) {
            // View/Edit Mode
            // In a real app we might need to fetch details if not included in the list DTO
            // But our GetByProject returns them.
            if (payment.details) {
                setDetails(payment.details);
            }
        } else if (isOpen) {
            // Create Mode Reset
            setCreateForm({
                date: new Date().toISOString().split('T')[0],
                periodStart: new Date().toISOString().split('T')[0],
                periodEnd: new Date().toISOString().split('T')[0],
                materialOnSiteAmount: 0,
                advanceDeductionAmount: 0,
                isExpense: false
            });
            setDetails([]);
        }
    }, [isOpen, payment]);

    const handleCreate = async () => {
        try {
            setLoading(true);
            await projectService.payments.create({
                projectId,
                date: createForm.date!,
                periodStart: createForm.periodStart!,
                periodEnd: createForm.periodEnd!,
                materialOnSiteAmount: createForm.materialOnSiteAmount || 0,
                advanceDeductionAmount: createForm.advanceDeductionAmount || 0,
                isExpense: createForm.isExpense || false
            } as CreateProgressPaymentDto);
            onSaved();
            onClose();
        } catch (error) {
            console.error('Failed to create payment', error);
        } finally {
            setLoading(false);
        }
    };

    const handleUpdateDetail = async (detail: ProgressPaymentDetailDto, newQuantity: number) => {
        try {
            // Optimistic update
            const updatedDetails = details.map(d => d.id === detail.id ? { ...d, cumulativeQuantity: newQuantity } : d);
            setDetails(updatedDetails);

            await projectService.payments.updateDetail(projectId, payment!.id, {
                id: detail.id,
                cumulativeQuantity: newQuantity
            });

            onSaved();
            setEditingDetailId(null);
        } catch (error) {
            console.error('Failed to update detail', error);
            // Revert logic could differ, but simple error log for now
        }
    };

    // Quick inline edit handler
    const startEdit = (detail: ProgressPaymentDetailDto) => {
        if (payment?.status !== ProgressPaymentStatus.Draft) return;
        setEditingDetailId(detail.id);
        setTempQuantity(detail.cumulativeQuantity);
    };

    const saveEdit = async (detail: ProgressPaymentDetailDto) => {
        await handleUpdateDetail(detail, tempQuantity);
    };

    const formatCurrency = (val: number) => new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(val);

    return (
        <Modal
            isOpen={isOpen}
            onClose={onClose}
            title={payment ? t('projects.payments.paymentTitle', { number: payment.paymentNo }) : t('projects.payments.createHakedis')}
            size="xl"
        >
            <div className="space-y-6 py-4">
                {/* Header Info */}
                {!payment ? (
                    <div className="grid grid-cols-3 gap-4">
                        <Input
                            label={t('common.date')}
                            type="date"
                            value={createForm.date}
                            onChange={(e) => setCreateForm({ ...createForm, date: e.target.value })}
                        />
                        <Input
                            label={t('common.periodStart')}
                            type="date"
                            value={createForm.periodStart}
                            onChange={(e) => setCreateForm({ ...createForm, periodStart: e.target.value })}
                        />
                        <Input
                            label={t('common.periodEnd')}
                            type="date"
                            value={createForm.periodEnd}
                            onChange={(e) => setCreateForm({ ...createForm, periodEnd: e.target.value })}
                        />
                        <Input
                            label={t('projects.payments.materialOnSite')}
                            type="number"
                            value={createForm.materialOnSiteAmount}
                            onChange={(e) => setCreateForm({ ...createForm, materialOnSiteAmount: parseFloat(e.target.value) })}
                        />
                        <Input
                            label={t('projects.payments.advanceDeduction')}
                            type="number"
                            value={createForm.advanceDeductionAmount}
                            onChange={(e) => setCreateForm({ ...createForm, advanceDeductionAmount: parseFloat(e.target.value) })}
                        />
                    </div>
                ) : (
                    <div className="grid grid-cols-4 gap-4 bg-muted/30 p-4 rounded-lg">
                        <div>
                            <div className="text-sm text-muted-foreground">{t('common.date')}</div>
                            <div className="font-medium">{new Date(payment.date).toLocaleDateString()}</div>
                        </div>
                        <div>
                            <div className="text-sm text-muted-foreground">{t('projects.payments.status')}</div>
                            <div className={`font-medium ${payment.status === ProgressPaymentStatus.Approved ? 'text-green-600' : 'text-yellow-600'
                                }`}>
                                {payment.status === ProgressPaymentStatus.Draft ? t('projects.payments.statuses.draft') : t('projects.payments.statuses.approved')}
                            </div>
                        </div>
                        <div>
                            <div className="text-sm text-muted-foreground">{t('projects.payments.grossAmount')}</div>
                            <div className="font-medium">{formatCurrency(payment.grossWorkAmount)}</div>
                        </div>
                        <div>
                            <div className="text-sm text-muted-foreground">{t('projects.payments.netAmount')}</div>
                            <div className="font-bold text-primary">{formatCurrency(payment.netPayableAmount)}</div>
                        </div>
                    </div>
                )}

                {/* Details Grid (Green Book) */}
                {payment && (
                    <div className="border rounded-md max-h-[500px] overflow-auto">
                        <table className="w-full text-sm">
                            <thead className="sticky top-0 bg-muted/90 backdrop-blur-sm z-10">
                                <tr className="border-b">
                                    <th className="p-2 text-left w-[100px]">{t('projects.payments.grid.code')}</th>
                                    <th className="p-2 text-left">{t('projects.payments.grid.description')}</th>
                                    <th className="p-2 text-right w-[100px]">{t('projects.payments.grid.prevQty')}</th>
                                    <th className="p-2 text-right w-[120px]">{t('projects.payments.grid.cumulQty')}</th>
                                    <th className="p-2 text-right w-[100px]">{t('projects.payments.grid.periodQty')}</th>
                                    <th className="p-2 text-right w-[100px]">{t('projects.payments.grid.price')}</th>
                                    <th className="p-2 text-right w-[120px]">{t('projects.payments.grid.periodAmt')}</th>
                                    <th className="p-2 text-right w-[120px]">{t('projects.payments.grid.totalAmt')}</th>
                                </tr>
                            </thead>
                            <tbody className="divide-y">
                                {details.map(detail => (
                                    <tr key={detail.id} className="hover:bg-muted/30 transition-colors">
                                        <td className="p-2 font-mono text-xs text-muted-foreground">{detail.itemCode}</td>
                                        <td className="p-2">{detail.description}</td>
                                        <td className="p-2 text-right text-muted-foreground">{detail.previousCumulativeQuantity}</td>
                                        <td className="p-2 text-right">
                                            {editingDetailId === detail.id ? (
                                                <div className="flex items-center space-x-1 justify-end">
                                                    <input
                                                        className="w-20 px-1 py-0.5 text-right border rounded bg-background focus:ring-1 focus:ring-primary outline-none"
                                                        type="number"
                                                        value={tempQuantity}
                                                        onChange={(e) => setTempQuantity(parseFloat(e.target.value))}
                                                        onKeyDown={(e) => {
                                                            if (e.key === 'Enter') saveEdit(detail);
                                                            if (e.key === 'Escape') setEditingDetailId(null);
                                                        }}
                                                        autoFocus
                                                    />
                                                    <button onClick={() => saveEdit(detail)} className="text-green-600 hover:bg-green-100 p-0.5 rounded">
                                                        <Check className="h-4 w-4" />
                                                    </button>
                                                </div>
                                            ) : (
                                                <div
                                                    onClick={() => startEdit(detail)}
                                                    className={`cursor-pointer px-2 py-0.5 rounded ${payment.status === ProgressPaymentStatus.Draft ? 'hover:bg-muted hover:border' : ''}`}
                                                >
                                                    {detail.cumulativeQuantity}
                                                </div>
                                            )}
                                        </td>
                                        <td className="p-2 text-right text-blue-600 font-medium">{detail.periodQuantity}</td>
                                        <td className="p-2 text-right text-muted-foreground">{formatCurrency(detail.unitPrice)}</td>
                                        <td className="p-2 text-right">{formatCurrency(detail.periodAmount)}</td>
                                        <td className="p-2 text-right font-medium">{formatCurrency(detail.totalAmount)}</td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                    </div>
                )}

                {/* Footer Actions */}
                <div className="flex justify-end space-x-2 pt-4 border-t">
                    <Button variant="ghost" onClick={onClose}>{t('common.close')}</Button>
                    {!payment && (
                        <Button onClick={handleCreate} disabled={loading}>
                            {loading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                            {t('common.create')}
                        </Button>
                    )}
                </div>
            </div>
        </Modal>
    );
}
