import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, ArrowRightLeft, Landmark, CheckCircle, Ban } from 'lucide-react';
import { api } from '../../services/api';
import { Button, DataTable, useToast } from '@/components/ui';
import { ChequeStatusBadge, ChequeTypeBadge } from './components/ChequeUtils';
import { ChequeActionDialog } from './components/ChequeActionDialog';
import { CreateChequeDialog } from './components/CreateChequeDialog';

export const ChequesPage = () => {
    const { t } = useTranslation();
    const toast = useToast();
    const [cheques, setCheques] = useState<any[]>([]);
    const [loading, setLoading] = useState(true);
    const [isCreateOpen, setIsCreateOpen] = useState(false);
    const [selectedCheque, setSelectedCheque] = useState<any>(null);
    const [actionType, setActionType] = useState<string>(''); // 'endorse', 'bank', 'paid', 'bounce'

    const fetchCheques = async () => {
        setLoading(true);
        const res = await api.finance.cheques.getAll();
        if (res.success && res.data) {
            setCheques(res.data);
        } else {
            toast.error('Error', res.error);
        }
        setLoading(false);
    };

    useEffect(() => {
        fetchCheques();
    }, []);

    const handleAction = (cheque: any, action: string) => {
        setSelectedCheque(cheque);
        setActionType(action);
    };

    const columns = [
        {
            key: 'chequeNumber',
            header: t('cheques.chequeNumber', 'Number')
        },
        {
            key: 'type',
            header: t('cheques.typeLabel', 'Type'),
            render: (row: any) => <ChequeTypeBadge type={row.type} />
        },
        {
            key: 'bankName',
            header: t('cheques.bankName', 'Bank')
        },
        {
            key: 'dueDate',
            header: t('cheques.dueDate', 'Due Date'),
            render: (row: any) => new Date(row.dueDate).toLocaleDateString()
        },
        {
            key: 'amount',
            header: t('cheques.amount', 'Amount'),
            render: (row: any) => row.amount.toFixed(2)
        },
        {
            key: 'status',
            header: t('cheques.statusLabel', 'Status'),
            render: (row: any) => <ChequeStatusBadge status={row.currentStatus} />
        },
        {
            key: 'actions',
            header: t('common.actions', 'Actions'),
            render: (row: any) => (
                <div className="flex gap-1 justify-end">
                    {/* Only show actions if in Portfolio or Endorsed/Bank */}
                    {row.currentStatus === 0 && ( // Portfolio
                        <>
                            <Button size="sm" variant="secondary" title={t('cheques.actions.endorse', 'Endorse')} onClick={() => handleAction(row, 'endorse')}>
                                <ArrowRightLeft className="w-4 h-4" />
                            </Button>
                            <Button size="sm" variant="secondary" title={t('cheques.actions.toBank', 'To Bank')} onClick={() => handleAction(row, 'bank')}>
                                <Landmark className="w-4 h-4" />
                            </Button>
                        </>
                    )}
                    {(row.currentStatus === 1 || row.currentStatus === 2 || row.currentStatus === 3) && ( // Endorsed, Bank, Pledged
                        <Button size="sm" variant="secondary" title={t('cheques.actions.markPaid', 'Mark Paid')} onClick={() => handleAction(row, 'paid')}>
                            <CheckCircle className="w-4 h-4" />
                        </Button>
                    )}
                    {row.currentStatus !== 4 && row.currentStatus !== 5 && row.currentStatus !== 6 && (
                        <Button size="sm" variant="danger" title={t('cheques.actions.markBounced', 'Bounce')} onClick={() => handleAction(row, 'bounce')}>
                            <Ban className="w-4 h-4" />
                        </Button>
                    )}
                </div>
            )
        }
    ];

    return (
        <div className="space-y-6">
            <div className="flex justify-between items-center">
                <h1 className="text-3xl font-bold tracking-tight">{t('cheques.title', 'Cheques / Promissory Notes')}</h1>
                <Button onClick={() => setIsCreateOpen(true)}>
                    <Plus className="mr-2 h-4 w-4" /> {t('cheques.new', 'New Cheque')}
                </Button>
            </div>

            <div className="bg-[hsl(var(--card))] rounded-xl border shadow-sm">
                <div className="p-6 border-b border-[hsl(var(--border))]">
                    <h3 className="text-lg font-semibold">{t('cheques.list', 'All Cheques')}</h3>
                </div>
                <div className="p-6">
                    <DataTable
                        columns={columns}
                        data={cheques}
                        isLoading={loading}
                        keyField="id"
                    />
                </div>
            </div>

            <CreateChequeDialog
                open={isCreateOpen}
                onOpenChange={setIsCreateOpen}
                onSuccess={fetchCheques}
            />

            {selectedCheque && (
                <ChequeActionDialog
                    open={!!selectedCheque}
                    onOpenChange={(op: boolean) => !op && setSelectedCheque(null)}
                    cheque={selectedCheque}
                    actionType={actionType}
                    onSuccess={() => {
                        setSelectedCheque(null);
                        fetchCheques();
                    }}
                />
            )}
        </div>
    );
};
