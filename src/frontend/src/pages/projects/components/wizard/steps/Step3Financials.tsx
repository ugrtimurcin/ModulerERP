import { Input } from '@/components/ui';
import { useTranslation } from 'react-i18next';
import type { CreateProgressPaymentDto, ProgressPaymentDto } from '@/types/project';

interface Step3Props {
    data: Partial<CreateProgressPaymentDto>;
    onChange: (data: Partial<CreateProgressPaymentDto>) => void;
    payment: ProgressPaymentDto;
}

export function Step3Financials({ data, onChange, payment }: Step3Props) {
    const { t } = useTranslation();

    const handleChange = (field: keyof CreateProgressPaymentDto, value: any) => {
        onChange({ ...data, [field]: value });
    };

    const formatCurrency = (val: number) => new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(val);

    return (
        <div className="space-y-6 max-w-2xl mx-auto py-4">
            <h3 className="text-lg font-medium text-center">{t('projects.payments.wizard.financialAdjustments')}</h3>

            {/* Summary Card */}
            <div className="bg-muted/30 p-4 rounded-lg border">
                <div className="grid grid-cols-2 gap-4 text-sm">
                    <div>
                        <div className="text-muted-foreground">{t('projects.payments.grossAmount')}</div>
                        <div className="text-xl font-semibold">{formatCurrency(payment.grossWorkAmount)}</div>
                    </div>
                    <div>
                        <div className="text-muted-foreground">{t('projects.payments.previousCumulative')}</div>
                        <div className="font-medium">{formatCurrency(payment.previousCumulativeAmount)}</div>
                    </div>
                </div>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div className="space-y-4">
                    <h4 className="font-medium text-sm text-muted-foreground border-b pb-2">{t('projects.payments.deductions')}</h4>

                    <Input
                        label={t('projects.payments.advanceDeduction')}
                        type="number"
                        value={data.advanceDeductionAmount}
                        onChange={(e) => handleChange('advanceDeductionAmount', parseFloat(e.target.value) || 0)}
                    />

                    <Input
                        label={t('projects.payments.materialOnSite')}
                        type="number"
                        value={data.materialOnSiteAmount}
                        onChange={(e) => handleChange('materialOnSiteAmount', parseFloat(e.target.value) || 0)}
                        hint={t('projects.payments.materialOnSiteDesc')}
                    />
                </div>

                <div className="space-y-4">
                    <h4 className="font-medium text-sm text-muted-foreground border-b pb-2">{t('projects.payments.calculatedDeductions')}</h4>

                    <div className="flex justify-between items-center p-2 bg-red-50 rounded">
                        <span>{t('projects.payments.retention')} ({payment.retentionRate}%)</span>
                        <span className="font-medium text-red-700">-{formatCurrency(payment.retentionAmount)}</span>
                    </div>

                    <div className="flex justify-between items-center p-2 bg-red-50 rounded">
                        <span>{t('projects.payments.withholdingTax')} ({payment.withholdingTaxRate}%)</span>
                        <span className="font-medium text-red-700">-{formatCurrency(payment.withholdingTaxAmount)}</span>
                    </div>
                </div>
            </div>

            <div className="border-t pt-4">
                <div className="flex justify-between items-center text-lg">
                    <span className="font-bold">{t('projects.payments.netAmount')}</span>
                    <span className="font-bold text-green-700 text-2xl">{formatCurrency(payment.netPayableAmount)}</span>
                </div>
            </div>
        </div>
    );
}
