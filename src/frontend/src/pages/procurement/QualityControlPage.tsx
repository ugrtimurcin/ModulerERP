import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Search, ClipboardCheck } from 'lucide-react';
import { DataTable, Button, Badge, useToast } from '@/components/ui';
import type { Column } from '@/components/ui';
import { api } from '@/lib/api';
import { QualityControlStatus } from '@/types/procurement';
import { QualityControlDialog } from './QualityControlDialog';

export function QualityControlPage() {
    const { t } = useTranslation();
    const toast = useToast();

    const [inspections, setInspections] = useState<any[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [isDialogOpen, setIsDialogOpen] = useState(false);

    const loadData = useCallback(async () => {
        setIsLoading(true);
        try {
            const data = await api.get<any[]>('/procurement/qc');
            setInspections(data);
        } catch {
            // Toast suppressed for cleaner initial load if empty
            setInspections([]);
        }
        setIsLoading(false);
    }, [toast]);

    useEffect(() => { loadData(); }, [loadData]);

    const getStatusBadge = (status: number) => {
        switch (status) {
            case QualityControlStatus.Pending:
                return <Badge variant="warning">{t('procurement.status.pending')}</Badge>;
            case QualityControlStatus.Passed:
                return <Badge variant="success">{t('procurement.status.passed')}</Badge>;
            case QualityControlStatus.Failed:
                return <Badge variant="warning">{t('procurement.status.failed')}</Badge>;
            case QualityControlStatus.ConditionallyAccepted:
                return <Badge variant="info">{t('procurement.status.conditional')}</Badge>;
            default:
                return <Badge variant="default">{t('common.unknown')}</Badge>;
        }
    };

    const columns: Column<any>[] = [
        {
            key: 'id',
            header: t('common.id'),
            render: (qc) => (
                <div className="flex items-center gap-3">
                    <div className="w-10 h-10 rounded-lg bg-gradient-to-br from-purple-500 to-violet-500 flex items-center justify-center text-white">
                        <ClipboardCheck className="w-5 h-5" />
                    </div>
                    <div>
                        <p className="font-mono font-semibold truncate w-24">{qc.id.substring(0, 8)}</p>
                        <p className="text-xs text-[hsl(var(--muted-foreground))]">
                            {new Date(qc.inspectionDate).toLocaleDateString()}
                        </p>
                    </div>
                </div>
            ),
        },
        {
            key: 'inspector',
            header: t('procurement.inspector'),
            render: (qc) => <span className="font-medium">{qc.inspectorName || 'Unknown'}</span>,
        },
        {
            key: 'quantityPassed',
            header: t('procurement.passed'),
            render: (qc) => <span className="font-mono text-green-600">{qc.quantityPassed}</span>,
        },
        {
            key: 'quantityRejected',
            header: t('procurement.rejected'),
            render: (qc) => <span className="font-mono text-red-600">{qc.quantityRejected}</span>,
        },
        {
            key: 'status',
            header: t('common.status'),
            render: (qc) => getStatusBadge(qc.status),
        },
    ];

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between">
                <div>
                    <h1 className="text-2xl font-bold flex items-center gap-2">
                        <ClipboardCheck className="w-6 h-6" />
                        {t('procurement.qualityControl')}
                    </h1>
                    <p className="text-[hsl(var(--muted-foreground))]">{t('procurement.qcSubtitle')}</p>
                </div>
                <Button onClick={() => setIsDialogOpen(true)}>
                    <Plus className="w-4 h-4" />
                    {t('procurement.createInspection')}
                </Button>
            </div>

            <DataTable
                data={inspections}
                columns={columns}
                keyField="id"
                isLoading={isLoading}
                searchable
                searchPlaceholder={t('common.search')}
                actions={(qc) => (
                    <div className="flex items-center gap-1">
                        <button className="p-2 rounded-lg hover:bg-[hsl(var(--accent))]" title={t('common.view')} onClick={() => console.log(qc)}>
                            <Search className="w-4 h-4" />
                        </button>
                    </div>
                )}
            />

            <QualityControlDialog
                open={isDialogOpen}
                onClose={(saved) => {
                    setIsDialogOpen(false);
                    if (saved) loadData();
                }}
                qc={null}
            />
        </div>
    );
}
