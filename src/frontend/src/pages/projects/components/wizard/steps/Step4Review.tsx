import { useTranslation } from 'react-i18next';
import type { ProgressPaymentDto } from '@/types/project';

interface Step4Props {
    payment: ProgressPaymentDto;
}

export function Step4Review({ payment }: Step4Props) {
    const { t } = useTranslation();
    const formatCurrency = (val: number) => new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(val);

    return (
        <div className="space-y-6 max-w-2xl mx-auto py-4">
            <div className="text-center">
                <div className="w-16 h-16 bg-green-100 text-green-700 rounded-full flex items-center justify-center mx-auto mb-4">
                    <span className="text-2xl font-bold">✓</span>
                </div>
                <h3 className="text-2xl font-bold">{t('projects.payments.wizard.readyToFinish')}</h3>
                <p className="text-muted-foreground mt-2">{t('projects.payments.wizard.reviewNote')}</p>
            </div>

            <div className="bg-card border rounded-lg overflow-hidden shadow-sm">
                <div className="p-6 space-y-4">
                    <div className="grid grid-cols-2 gap-4 border-b pb-4">
                        <div>
                            <div className="text-sm text-muted-foreground">{t('projects.payments.paymentNo')}</div>
                            <div className="font-medium">#{payment.paymentNo}</div>
                        </div>
                        <div>
                            <div className="text-sm text-muted-foreground">{t('common.date')}</div>
                            <div className="font-medium">{new Date(payment.date).toLocaleDateString()}</div>
                        </div>
                        <div>
                            <div className="text-sm text-muted-foreground">{t('common.period')}</div>
                            <div className="font-medium">{new Date(payment.periodStart).toLocaleDateString()} - {new Date(payment.periodEnd).toLocaleDateString()}</div>
                        </div>
                    </div>

                    <div className="space-y-2">
                        <div className="flex justify-between">
                            <span>{t('projects.payments.grossAmount')}</span>
                            <span className="font-medium">{formatCurrency(payment.grossWorkAmount)}</span>
                        </div>
                        <div className="flex justify-between text-red-600 text-sm">
                            <span>{t('projects.payments.retention')}</span>
                            <span>-{formatCurrency(payment.retentionAmount)}</span>
                        </div>
                        <div className="flex justify-between text-red-600 text-sm">
                            <span>{t('projects.payments.advanceDeduction')}</span>
                            <span>-{formatCurrency(payment.advanceDeductionAmount)}</span>
                        </div>
                        <div className="flex justify-between text-red-600 text-sm">
                            <span>{t('projects.payments.withholdingTax')}</span>
                            <span>-{formatCurrency(payment.withholdingTaxAmount)}</span>
                        </div>

                        <div className="border-t pt-2 mt-2 flex justify-between items-center text-lg font-bold">
                            <span>{t('projects.payments.netPayable')}</span>
                            <span className="text-green-700">{formatCurrency(payment.netPayableAmount)}</span>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
}
