import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Eye, FileText } from 'lucide-react';
import { DataTable, Button, Badge, useToast } from '@/components/ui';
import type { Column } from '@/components/ui';
import { api } from '@/lib/api';
import { RfqStatus } from '@/types/procurement';

import { RfqDialog } from './RfqDialog';

export function RFQsPage() {
    const { t } = useTranslation();
    const toast = useToast();

    const [rfqs, setRfqs] = useState<any[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [isDialogOpen, setIsDialogOpen] = useState(false);

    const loadData = useCallback(async () => {
        setIsLoading(true);
        try {
            const data = await api.get<any[]>('/procurement/rfqs');
            setRfqs(data);
        } catch {
            toast.error('Failed to load RFQs');
        }
        setIsLoading(false);
    }, [toast]);

    useEffect(() => { loadData(); }, [loadData]);

    const getStatusBadge = (status: number) => {
        switch (status) {
            case RfqStatus.Open:
                return <Badge variant="info">{t('procurement.status.open')}</Badge>;
            case RfqStatus.Closed:
                return <Badge variant="warning">{t('procurement.status.closed')}</Badge>;
            case RfqStatus.Awarded:
                return <Badge variant="success">{t('procurement.status.awarded')}</Badge>;
            default:
                return <Badge variant="default">{t('common.unknown')}</Badge>;
        }
    };

    const columns: Column<any>[] = [
        {
            key: 'rfqNumber',
            header: t('procurement.rfqNumber'),
            render: (rfq) => (
                <div className="flex items-center gap-3">
                    <div className="w-10 h-10 rounded-lg bg-gradient-to-br from-blue-500 to-indigo-500 flex items-center justify-center text-white">
                        <FileText className="w-5 h-5" />
                    </div>
                    <div>
                        <p className="font-mono font-semibold">{rfq.rfqNumber}</p>
                        <p className="text-xs text-[hsl(var(--muted-foreground))]">
                            {new Date(rfq.createdAt).toLocaleDateString()}
                        </p>
                    </div>
                </div>
            ),
        },
        {
            key: 'title',
            header: t('common.title'),
            render: (rfq) => <span className="font-medium">{rfq.title}</span>,
        },
        {
            key: 'deadLine',
            header: t('procurement.deadline'),
            render: (rfq) => (
                <span className="text-sm">{new Date(rfq.deadLine).toLocaleDateString()}</span>
            ),
        },
        {
            key: 'status',
            header: t('common.status'),
            render: (rfq) => getStatusBadge(rfq.status),
        },
    ];

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between">
                <div>
                    <h1 className="text-2xl font-bold flex items-center gap-2">
                        <FileText className="w-6 h-6" />
                        {t('procurement.rfqs')}
                    </h1>
                    <p className="text-[hsl(var(--muted-foreground))]">{t('procurement.rfqsSubtitle')}</p>
                </div>
                <Button onClick={() => setIsDialogOpen(true)}>
                    <Plus className="w-4 h-4" />
                    {t('procurement.createRfq')}
                </Button>
            </div>

            <DataTable
                data={rfqs}
                columns={columns}
                keyField="id"
                isLoading={isLoading}
                searchable
                searchPlaceholder={t('common.search')}
                actions={(rfq) => (
                    <div className="flex items-center gap-1">
                        <button className="p-2 rounded-lg hover:bg-[hsl(var(--accent))]" title={t('common.view')} onClick={() => console.log(rfq)}>
                            <Eye className="w-4 h-4" />
                        </button>
                    </div>
                )}
            />

            <RfqDialog
                open={isDialogOpen}
                onClose={(saved) => {
                    setIsDialogOpen(false);
                    if (saved) loadData();
                }}
                rfq={null} // Currently only create mode is supported properly
            />
        </div>
    );
}
