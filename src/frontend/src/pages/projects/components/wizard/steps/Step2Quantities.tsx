import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import type { ProgressPaymentDto, ProgressPaymentDetailDto } from '@/types/project';
import { projectService } from '@/services/projectService';
import { Check } from 'lucide-react';

interface Step2Props {
    payment: ProgressPaymentDto;
    projectId: string;
    onRefresh: () => void;
}

export function Step2Quantities({ payment, projectId, onRefresh }: Step2Props) {
    const { t } = useTranslation();
    const [editingId, setEditingId] = useState<string | null>(null);
    const [tempQty, setTempQty] = useState<number>(0);

    const formatCurrency = (val: number) => new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(val);

    const startEdit = (detail: ProgressPaymentDetailDto) => {
        setEditingId(detail.id);
        setTempQty(detail.cumulativeQuantity);
    };

    const saveEdit = async (detail: ProgressPaymentDetailDto) => {
        try {
            await projectService.payments.updateDetail(projectId, payment.id, {
                id: detail.id,
                cumulativeQuantity: tempQty
            });
            setEditingId(null);
            onRefresh();
        } catch (error) {
            console.error('Failed to save quantity', error);
        }
    };

    return (
        <div className="space-y-4">
            <div className="flex justify-between items-center mb-2">
                <h3 className="font-medium">{t('projects.payments.wizard.enterQuantities')}</h3>
                <span className="text-sm text-muted-foreground">{payment.details?.length || 0} items</span>
            </div>

            <div className="border rounded-md overflow-hidden">
                <table className="w-full text-sm">
                    <thead className="bg-muted text-muted-foreground">
                        <tr>
                            <th className="p-3 text-left">{t('projects.payments.grid.code')}</th>
                            <th className="p-3 text-left w-1/3">{t('projects.payments.grid.description')}</th>
                            <th className="p-3 text-right">{t('projects.payments.grid.prevQty')}</th>
                            <th className="p-3 text-right bg-blue-50/50">{t('projects.payments.grid.cumulQty')}</th>
                            <th className="p-3 text-right font-semibold text-blue-700">{t('projects.payments.grid.periodQty')}</th>
                            <th className="p-3 text-right">{t('projects.payments.grid.unitPrice')}</th>
                            <th className="p-3 text-right">{t('projects.payments.grid.totalAmt')}</th>
                        </tr>
                    </thead>
                    <tbody className="divide-y">
                        {payment.details?.map(detail => (
                            <tr key={detail.id} className="hover:bg-muted/30">
                                <td className="p-3 font-mono text-xs">{detail.itemCode}</td>
                                <td className="p-3">{detail.description}</td>
                                <td className="p-3 text-right text-muted-foreground">{detail.previousCumulativeQuantity}</td>
                                <td className="p-3 text-right bg-blue-50/30">
                                    {editingId === detail.id ? (
                                        <div className="flex items-center justify-end space-x-1">
                                            <input
                                                autoFocus
                                                type="number"
                                                className="w-24 text-right border rounded px-1 py-0.5"
                                                value={tempQty}
                                                onChange={e => setTempQty(parseFloat(e.target.value) || 0)}
                                                onKeyDown={e => {
                                                    if (e.key === 'Enter') saveEdit(detail);
                                                    if (e.key === 'Escape') setEditingId(null);
                                                }}
                                            />
                                            <button onClick={() => saveEdit(detail)} className="text-green-600 hover:bg-green-100 p-1 rounded">
                                                <Check className="w-4 h-4" />
                                            </button>
                                        </div>
                                    ) : (
                                        <div
                                            className="cursor-pointer hover:bg-blue-100 px-2 py-1 rounded inline-flex items-center gap-2 justify-end w-full"
                                            onClick={() => startEdit(detail)}
                                        >
                                            <span>{detail.cumulativeQuantity}</span>
                                            {/* <Edit2 className="w-3 h-3 text-muted-foreground opacity-50" /> */}
                                        </div>
                                    )}
                                </td>
                                <td className="p-3 text-right font-medium text-blue-700">{detail.periodQuantity}</td>
                                <td className="p-3 text-right text-muted-foreground">{formatCurrency(detail.unitPrice)}</td>
                                <td className="p-3 text-right">{formatCurrency(detail.totalAmount)}</td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>
            <div className="text-right text-xs text-muted-foreground">
                * Click on Cumulative Quantity to edit
            </div>
        </div>
    );
}
