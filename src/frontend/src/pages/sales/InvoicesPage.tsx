import React, { useEffect, useState, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Pencil, CheckCircle, FileText } from 'lucide-react';
import { api } from '../../services/api';
import { DataTable, Button, Badge, useToast, useDialog } from '@/components/ui';
import type { Column } from '@/components/ui';
import InvoiceDialog from './InvoiceDialog';

interface Invoice {
    id: string;
    invoiceNumber: string;
    partnerName: string;
    invoiceDate: string;
    dueDate: string;
    status: number;
    totalAmount: number;
    balanceDue: number;
    currencyCode: string; // Ensure backend returns this or DTO has it
}

const InvoicesPage: React.FC = () => {
    const { t } = useTranslation();
    const toast = useToast();
    const dialog = useDialog();

    const [invoices, setInvoices] = useState<Invoice[]>([]);
    const [loading, setLoading] = useState(true);
    const [page, setPage] = useState(1);
    const [total, setTotal] = useState(0);
    const pageSize = 20;

    // Dialog state
    const [isDialogOpen, setIsDialogOpen] = useState(false);
    const [selectedId, setSelectedId] = useState<string | undefined>(undefined);

    const loadInvoices = useCallback(async () => {
        setLoading(true);
        try {
            const res = await api.invoices.getAll(page, pageSize);
            if (res.success && res.data) {
                setInvoices(res.data.data);
                setTotal(res.data.totalCount);
            }
        } catch (error) {
            console.error('Failed to load invoices', error);
            toast.error(t('common.error'), 'Failed to load invoices');
        } finally {
            setLoading(false);
        }
    }, [page, toast]);

    useEffect(() => {
        loadInvoices();
    }, [loadInvoices]);

    const handleCreate = () => {
        setSelectedId(undefined);
        setIsDialogOpen(true);
    };

    const handleEdit = (invoice: Invoice) => {
        setSelectedId(invoice.id);
        setIsDialogOpen(true);
    };

    const handleIssue = async (invoice: Invoice) => {
        const confirmed = await dialog.confirm({
            title: 'Issue Invoice',
            message: 'Are you sure you want to issue this invoice? Finance entries will be created.',
            confirmText: 'Issue',
            cancelText: 'Cancel'
        });

        if (!confirmed) return;

        try {
            const res = await api.invoices.issue(invoice.id);
            if (res.success) {
                toast.success('Invoice issued successfully');
                loadInvoices();
            } else {
                toast.error('Failed to issue invoice', res.error);
            }
        } catch (error) {
            console.error('Issue failed', error);
            toast.error('Failed to issue invoice');
        }
    };

    const getStatusVariant = (status: number) => {
        switch (status) {
            case 0: return 'default'; // Draft
            case 1: return 'info'; // Issued
            case 2: return 'warning'; // Partially Paid
            case 3: return 'success'; // Paid
            case 4: return 'error'; // Overdue
            case 5: return 'default'; // Cancelled
            default: return 'default';
        }
    };

    const getStatusLabel = (status: number) => {
        switch (status) {
            case 0: return 'Draft';
            case 1: return 'Issued';
            case 2: return 'Partially Paid';
            case 3: return 'Paid';
            case 4: return 'Overdue';
            case 5: return 'Cancelled';
            default: return 'Unknown';
        }
    };

    const columns: Column<Invoice>[] = [
        {
            key: 'invoiceNumber',
            header: 'Number',
            render: (row) => (
                <div className="flex items-center gap-2">
                    <FileText className="w-4 h-4 text-gray-500" />
                    <span className="font-medium text-blue-600 cursor-pointer hover:underline" onClick={() => handleEdit(row)}>
                        {row.invoiceNumber}
                    </span>
                </div>
            )
        },
        {
            key: 'partnerName',
            header: 'Partner',
            render: (row) => row.partnerName || 'Unknown' // Ideally link to partner
        },
        {
            key: 'invoiceDate',
            header: 'Date',
            render: (row) => new Date(row.invoiceDate).toLocaleDateString()
        },
        {
            key: 'dueDate',
            header: 'Due Date',
            render: (row) => new Date(row.dueDate).toLocaleDateString()
        },
        {
            key: 'status',
            header: 'Status',
            render: (row) => (
                <Badge variant={getStatusVariant(row.status)}>
                    {getStatusLabel(row.status)}
                </Badge>
            )
        },
        {
            key: 'totalAmount',
            header: 'Total',
            align: 'right',
            render: (row) => `${row.totalAmount.toFixed(2)}` // TODO: Add Currency Symbol
        },
        {
            key: 'balanceDue',
            header: 'Balance',
            align: 'right',
            render: (row) => <span className="font-medium">{row.balanceDue.toFixed(2)}</span>
        }
    ];

    return (
        <div className="space-y-6">
            {/* Header */}
            <div className="flex items-center justify-between">
                <div>
                    <h1 className="text-2xl font-bold">Sales Invoices</h1>
                    <p className="text-[hsl(var(--muted-foreground))]">Manage sales invoices</p>
                </div>
                <Button onClick={handleCreate}>
                    <Plus className="w-4 h-4 mr-2" />
                    New Invoice
                </Button>
            </div>

            {/* Table */}
            <DataTable
                data={invoices}
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
                            title="Edit"
                        >
                            <Pencil className="w-4 h-4" />
                        </button>
                        {row.status === 0 && (
                            <button
                                onClick={() => handleIssue(row)}
                                className="p-2 rounded-lg hover:bg-green-100 dark:hover:bg-green-900/30 text-green-600 transition-colors"
                                title="Issue Invoice"
                            >
                                <CheckCircle className="w-4 h-4" />
                            </button>
                        )}
                    </div>
                )}
            />

            <InvoiceDialog
                isOpen={isDialogOpen}
                onClose={() => setIsDialogOpen(false)}
                onSave={loadInvoices}
                invoiceId={selectedId}
            />
        </div>
    );
};

export default InvoicesPage;
