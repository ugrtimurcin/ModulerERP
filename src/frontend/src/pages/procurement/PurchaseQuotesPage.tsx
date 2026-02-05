import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { Eye, Receipt, Plus } from 'lucide-react';
import { DataTable, Badge, Button } from '@/components/ui';
import type { Column } from '@/components/ui';
import { PurchaseQuoteStatus } from '@/types/procurement';
import { PurchaseQuoteDialog } from './PurchaseQuoteDialog';
import { api } from '@/services/api';

export function PurchaseQuotesPage() {
    const { t } = useTranslation();

    const [quotes, setQuotes] = useState<any[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [isDialogOpen, setIsDialogOpen] = useState(false);

    const loadData = useCallback(async () => {
        setIsLoading(true);
        try {
            const res = await api.purchaseQuotes.getAll();
            if (Array.isArray(res)) setQuotes(res);
            else if ((res as any).data) setQuotes((res as any).data);
        } catch {
            setQuotes([]);
        } finally {
            setIsLoading(false);
        }
    }, []);

    useEffect(() => { loadData(); }, [loadData]);

    const getStatusBadge = (status: number) => {
        switch (status) {
            case PurchaseQuoteStatus.Pending:
                return <Badge variant="info">{t('procurement.status.pending')}</Badge>;
            case PurchaseQuoteStatus.Accepted:
                return <Badge variant="success">{t('procurement.status.accepted')}</Badge>;
            case PurchaseQuoteStatus.Rejected:
                return <Badge variant="warning">{t('procurement.status.rejected')}</Badge>;
            default:
                return <Badge variant="default">{t('common.unknown')}</Badge>;
        }
    };

    const columns: Column<any>[] = [
        {
            key: 'quoteReference',
            header: t('procurement.quoteReference'),
            render: (quote) => (
                <div className="flex items-center gap-3">
                    <div className="w-10 h-10 rounded-lg bg-gradient-to-br from-green-500 to-emerald-500 flex items-center justify-center text-white">
                        <Receipt className="w-5 h-5" />
                    </div>
                    <div>
                        <p className="font-mono font-semibold">{quote.quoteReference}</p>
                        <p className="text-xs text-[hsl(var(--muted-foreground))]">
                            {new Date(quote.createdAt || new Date()).toLocaleDateString()}
                        </p>
                    </div>
                </div>
            ),
        },
        {
            key: 'supplier',
            header: t('procurement.supplier'),
            render: (quote) => <span className="font-medium">{quote.supplierName || 'N/A'}</span>,
        },
        {
            key: 'totalAmount',
            header: t('common.amount'),
            render: (quote) => (
                <span className="font-mono font-medium">
                    {quote.totalAmount?.toLocaleString('en-US', { minimumFractionDigits: 2 })}
                </span>
            ),
        },
        {
            key: 'status',
            header: t('common.status'),
            render: (quote) => getStatusBadge(quote.status),
        },
    ];

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between">
                <div>
                    <h1 className="text-2xl font-bold flex items-center gap-2">
                        <Receipt className="w-6 h-6" />
                        {t('procurement.purchaseQuotes')}
                    </h1>
                    <p className="text-[hsl(var(--muted-foreground))]">{t('procurement.purchaseQuotesSubtitle')}</p>
                </div>
                <Button onClick={() => setIsDialogOpen(true)}>
                    <Plus className="w-4 h-4" />
                    {t('procurement.createQuote')}
                </Button>
            </div>

            <DataTable
                data={quotes}
                columns={columns}
                keyField="id"
                isLoading={isLoading}
                searchable
                searchPlaceholder={t('common.search')}
                actions={(quote) => (
                    <div className="flex items-center gap-1">
                        <button className="p-2 rounded-lg hover:bg-[hsl(var(--accent))]" title={t('common.view')} onClick={() => console.log(quote)}>
                            <Eye className="w-4 h-4" />
                        </button>
                    </div>
                )}
            />

            <PurchaseQuoteDialog
                open={isDialogOpen}
                onClose={(saved) => {
                    setIsDialogOpen(false);
                    if (saved) loadData();
                }}
                quote={null}
            />
        </div>
    );
}
