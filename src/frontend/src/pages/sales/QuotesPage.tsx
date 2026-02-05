import React, { useEffect, useState, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Pencil, Trash2, FileText } from 'lucide-react';
import { api } from '../../services/api';
import { DataTable, Button, Badge, useDialog, useToast } from '@/components/ui';
import type { Column } from '@/components/ui';
import QuoteDialog from './QuoteDialog';

interface Quote {
    id: string;
    quoteNumber: string;
    revisionNumber: number;
    partnerName: string;
    partnerId: string;
    createdAt: string;
    validUntil: string | null;
    status: string;
    totalAmount: number;
    currencyCode: string;
}

const QuotesPage: React.FC = () => {
    const { t } = useTranslation();
    const dialog = useDialog();
    const toast = useToast();

    const [quotes, setQuotes] = useState<Quote[]>([]);
    const [loading, setLoading] = useState(true);
    const [page, setPage] = useState(1);
    const [total, setTotal] = useState(0);
    const pageSize = 20;

    // Dialot State
    const [isDialogOpen, setIsDialogOpen] = useState(false);
    const [selectedQuoteId, setSelectedQuoteId] = useState<string | undefined>(undefined);

    const loadQuotes = useCallback(async () => {
        setLoading(true);
        try {
            const response = await api.quotes.getAll(page, pageSize);
            if (response.success && response.data) {
                setQuotes(response.data.data);
                setTotal(response.data.totalCount);
            }
        } catch (error) {
            console.error('Failed to load quotes', error);
            toast.error(t('common.error'), 'Failed to load quotes');
        } finally {
            setLoading(false);
        }
    }, [page, toast]);

    useEffect(() => {
        loadQuotes();
    }, [loadQuotes]);

    const handleCreate = () => {
        setSelectedQuoteId(undefined);
        setIsDialogOpen(true);
    };

    const handleEdit = (quote: Quote) => {
        setSelectedQuoteId(quote.id);
        setIsDialogOpen(true);
    };

    const handleDelete = async (quote: Quote) => {
        const confirmed = await dialog.danger({
            title: t('quotes.deleteQuote'),
            message: t('quotes.confirmDeleteQuote'),
            confirmText: t('common.delete')
        });

        if (!confirmed) return;

        try {
            const response = await api.quotes.delete(quote.id);
            if (response.success) {
                toast.success(t('quotes.quoteDeleted'));
                loadQuotes();
            } else {
                toast.error(t('common.error'), response.error);
            }
        } catch (error) {
            console.error('Failed to delete quote', error);
            toast.error(t('common.error'));
        }
    };

    const getStatusVariant = (status: string) => {
        switch (status) {
            case 'Draft': return 'default';
            case 'Sent': return 'info';
            case 'Accepted': return 'success';
            case 'Rejected': return 'error';
            case 'Expired': return 'warning';
            case 'Converted': return 'info'; // Was primary
            case 'Cancelled': return 'default';
            default: return 'default';
        }
    };

    const columns: Column<Quote>[] = [
        {
            key: 'quoteNumber',
            header: t('quotes.quoteNumber'),
            render: (row) => (
                <div className="flex items-center gap-2">
                    <FileText className="w-4 h-4 text-gray-500" />
                    <span
                        className="font-medium text-blue-600 cursor-pointer hover:underline"
                        onClick={() => handleEdit(row)}
                    >
                        {row.quoteNumber}
                    </span>
                    <span className="text-xs text-gray-400">v{row.revisionNumber}</span>
                </div>
            )
        },
        {
            key: 'partnerName',
            header: t('quotes.partner'),
            render: (row) => row.partnerName || row.partnerId
        },
        {
            key: 'createdAt',
            header: t('quotes.date'),
            render: (row) => new Date(row.createdAt).toLocaleDateString()
        },
        {
            key: 'validUntil',
            header: t('quotes.validUntil'),
            render: (row) => row.validUntil ? new Date(row.validUntil).toLocaleDateString() : '-'
        },
        {
            key: 'status',
            header: t('quotes.status'),
            render: (row) => (
                <Badge variant={getStatusVariant(row.status)}>
                    {t(`quotes.statuses.${row.status}`, { defaultValue: row.status })}
                </Badge>
            )
        },
        {
            key: 'totalAmount',
            header: t('quotes.total'),
            align: 'right',
            render: (row) => `${row.totalAmount.toFixed(2)} ${row.currencyCode}`
        }
    ];

    return (
        <div className="space-y-6">
            {/* Header */}
            <div className="flex items-center justify-between">
                <div>
                    <h1 className="text-2xl font-bold">{t('quotes.title')}</h1>
                    <p className="text-[hsl(var(--muted-foreground))]">{t('quotes.subtitle')}</p>
                </div>
                <Button onClick={handleCreate}>
                    <Plus className="w-4 h-4 mr-2" />
                    {t('quotes.createQuote')}
                </Button>
            </div>

            {/* Table */}
            <DataTable
                data={quotes}
                columns={columns}
                keyField="id"
                isLoading={loading}
                pagination={{
                    page,
                    pageSize,
                    total,
                    onPageChange: setPage
                }}
                actions={(row) => (
                    <div className="flex items-center gap-1">
                        <button
                            onClick={() => handleEdit(row)}
                            className="p-2 rounded-lg hover:bg-[hsl(var(--accent))] transition-colors"
                            title={t('common.edit')}
                        >
                            <Pencil className="w-4 h-4" />
                        </button>
                        <button
                            onClick={() => handleDelete(row)}
                            className="p-2 rounded-lg hover:bg-red-100 dark:hover:bg-red-900/30 text-red-600 transition-colors"
                            title={t('common.delete')}
                        >
                            <Trash2 className="w-4 h-4" />
                        </button>
                    </div>
                )}
            />

            <QuoteDialog
                isOpen={isDialogOpen}
                onClose={() => setIsDialogOpen(false)}
                onSave={loadQuotes}
                quoteId={selectedQuoteId}
            />
        </div>
    );
};

export default QuotesPage;
