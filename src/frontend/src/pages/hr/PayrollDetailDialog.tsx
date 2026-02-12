import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { X, Printer } from 'lucide-react';
import { DataTable, Button, useToast } from '@/components/ui';
import type { Column } from '@/components/ui';
import { api } from '@/lib/api';

interface PayrollEntry {
    id: string;
    employeeName: string;
    baseSalary: number;
    overtimePay: number;
    commissionPay: number;
    socialSecurityEmployee: number;
    providentFundEmployee: number;
    incomeTax: number;
    advanceDeduction: number;
    netPayable: number;
}

interface PayrollDetailDialogProps {
    open: boolean;
    onClose: () => void;
    payrollRunId: string | null;
}

export function PayrollDetailDialog({ open, onClose, payrollRunId }: PayrollDetailDialogProps) {
    const { t } = useTranslation();
    const toast = useToast();
    const [entries, setEntries] = useState<PayrollEntry[]>([]);
    const [isLoading, setIsLoading] = useState(false);

    useEffect(() => {
        if (open && payrollRunId) {
            setIsLoading(true);
            const loadSlips = async () => {
                try {
                    const data = await api.get<PayrollEntry[]>(`/hr/payroll/${payrollRunId}/slips`);
                    setEntries(data);
                } catch {
                    toast.error(t('common.error'));
                } finally {
                    setIsLoading(false);
                }
            };
            loadSlips();
        } else {
            setEntries([]);
        }
    }, [open, payrollRunId, toast, t]);

    const formatCurrency = (amount: number) => {
        return new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(amount);
    };

    const columns: Column<PayrollEntry>[] = [
        {
            key: 'employeeName',
            header: t('hr.employee'),
            render: (rec) => <span className="font-medium">{rec.employeeName}</span>
        },
        {
            key: 'baseSalary',
            header: t('hr.baseSalary'),
            render: (rec) => formatCurrency(rec.baseSalary)
        },
        {
            key: 'overtimePay',
            header: t('hr.overtimePay'),
            render: (rec) => <span className="text-green-600">{formatCurrency(rec.overtimePay)}</span>
        },
        {
            key: 'commissionPay',
            header: t('hr.commission'),
            render: (rec) => <span className="text-green-600">{formatCurrency(rec.commissionPay)}</span>
        },
        {
            key: 'socialSecurityEmployee',
            header: t('hr.socialSecurity'),
            render: (rec) => <span className="text-red-600">-{formatCurrency(rec.socialSecurityEmployee)}</span>
        },
        {
            key: 'providentFundEmployee',
            header: t('hr.providentFund'),
            render: (rec) => <span className="text-red-600">-{formatCurrency(rec.providentFundEmployee)}</span>
        },
        {
            key: 'incomeTax',
            header: t('hr.incomeTax'),
            render: (rec) => <span className="text-red-600">-{formatCurrency(rec.incomeTax)}</span>
        },
        {
            key: 'advanceDeduction',
            header: t('hr.advanceDeduction'),
            render: (rec) => <span className="text-amber-600">-{formatCurrency(rec.advanceDeduction)}</span>
        },
        {
            key: 'netPayable',
            header: t('hr.netPayable'),
            render: (rec) => <span className="font-bold">{formatCurrency(rec.netPayable)}</span>
        }
    ];

    if (!open) return null;

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center">
            <div className="absolute inset-0 bg-black/50 backdrop-blur-sm" onClick={onClose} />
            <div className="relative bg-[hsl(var(--card))] rounded-xl shadow-2xl w-full max-w-6xl max-h-[90vh] overflow-hidden border border-[hsl(var(--border))] flex flex-col">
                <div className="bg-[hsl(var(--card))] px-6 py-4 border-b border-[hsl(var(--border))] flex items-center justify-between shrink-0">
                    <h2 className="text-xl font-semibold">
                        {t('hr.payrollDetails')}
                    </h2>
                    <div className="flex gap-2">
                        <Button variant="secondary" onClick={() => window.print()}>
                            <Printer className="w-4 h-4 mr-2" />
                            {t('common.print')}
                        </Button>
                        <button
                            onClick={onClose}
                            className="p-2 rounded-lg hover:bg-[hsl(var(--accent))] transition-colors"
                        >
                            <X className="w-5 h-5" />
                        </button>
                    </div>
                </div>

                <div className="p-6 overflow-auto flex-1">
                    <DataTable
                        data={entries}
                        columns={columns}
                        keyField="id"
                        isLoading={isLoading}
                        searchable
                        searchPlaceholder={t('common.search')}
                    />
                </div>
            </div>
        </div>
    );
}
