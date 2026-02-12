import { useState, useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import {
    useReactTable,
    getCoreRowModel,
    getPaginationRowModel,
    getSortedRowModel,
    getFilteredRowModel,
    flexRender,
    createColumnHelper,
    type RowSelectionState,
    type SortingState,
} from '@tanstack/react-table';
import { CheckCircle, ArrowUpDown } from 'lucide-react';
import { Button } from '@/components/ui';
import { useToast } from '@/components/ui/Toast';
import { api } from '@/lib/api';
import type { DailyLogDto } from '@/types/project';

interface DailyLogDataGridProps {
    logs: DailyLogDto[];
    onRefresh: () => void;
}

export function DailyLogDataGrid({ logs, onRefresh }: DailyLogDataGridProps) {
    const { t } = useTranslation();
    const toast = useToast();
    const [sorting, setSorting] = useState<SortingState>([]);
    const [rowSelection, setRowSelection] = useState<RowSelectionState>({});

    const handleApprove = async (id: string) => {
        if (!window.confirm(t('projects.dailyLogs.confirmApprove'))) return;
        try {
            await api.post(`/dailylog/${id}/approve`, {});
            toast.success(t('common.success'), t('projects.dailyLogs.approved'));
            onRefresh();
        } catch (error) {
            console.error('Failed to approve', error);
            toast.error(t('common.error'), t('common.errorSaving'));
        }
    };

    const handleBatchApprove = async () => {
        const idsToApprove = table.getSelectedRowModel().rows
            .filter(row => !row.original.isApproved)
            .map(row => row.original.id);

        if (idsToApprove.length === 0) return;

        if (!window.confirm(t('projects.dailyLogs.confirmBatchApprove', { count: idsToApprove.length }))) return;

        try {
            await Promise.all(idsToApprove.map(id => api.post(`/dailylog/${id}/approve`, {})));
            toast.success(t('common.success'), t('projects.dailyLogs.batchApproved'));
            setRowSelection({});
            onRefresh();
        } catch (error) {
            console.error('Batch approve failed', error);
            toast.error(t('common.error'), t('common.errorSaving'));
        }
    };

    const columnHelper = createColumnHelper<DailyLogDto>();

    const columns = useMemo(() => [
        {
            id: 'select',
            header: ({ table }: { table: any }) => (
                <input
                    type="checkbox"
                    checked={table.getIsAllPageRowsSelected()}
                    onChange={table.getToggleAllPageRowsSelectedHandler()}
                    className="translate-y-[2px]"
                />
            ),
            cell: ({ row }: { row: any }) => (
                <input
                    type="checkbox"
                    checked={row.getIsSelected()}
                    disabled={row.original.isApproved}
                    onChange={row.getToggleSelectedHandler()}
                    className="translate-y-[2px]"
                />
            ),
            enableSorting: false,
            enableHiding: false,
        },
        columnHelper.accessor('date', {
            header: ({ column }) => {
                return (
                    <Button variant="ghost" onClick={() => column.toggleSorting(column.getIsSorted() === 'asc')}>
                        {t('common.date')}
                        <ArrowUpDown className="ml-2 h-4 w-4" />
                    </Button>
                )
            },
            cell: info => new Date(info.getValue()).toLocaleDateString(),
        }),
        columnHelper.accessor('weatherCondition', {
            header: t('projects.dailyLogs.weather'),
            cell: info => info.getValue(),
        }),
        columnHelper.accessor(row => row.resourceUsages?.length || 0, {
            id: 'resources',
            header: t('projects.dailyLogs.resources'),
            cell: info => info.getValue(),
        }),
        columnHelper.accessor('isApproved', {
            header: t('common.status'),
            cell: info => (
                <span className={`px-2 py-1 rounded-full text-xs font-medium ${info.getValue() ? 'bg-green-100 text-green-700' : 'bg-yellow-100 text-yellow-700'}`}>
                    {info.getValue() ? t('common.approved') : t('common.draft')}
                </span>
            ),
        }),
        columnHelper.display({
            id: 'actions',
            cell: ({ row }: { row: any }) => {
                const log = row.original;
                return (
                    <div className="flex items-center gap-2">
                        {!log.isApproved && (
                            <Button variant="ghost" size="sm" onClick={() => handleApprove(log.id)} className="text-green-600 hover:text-green-700 hover:bg-green-50">
                                <CheckCircle className="h-4 w-4" />
                            </Button>
                        )}
                    </div>
                );
            },
        }),
    ], [t]);

    const table = useReactTable({
        data: logs,
        columns,
        getCoreRowModel: getCoreRowModel(),
        getPaginationRowModel: getPaginationRowModel(),
        getSortedRowModel: getSortedRowModel(),
        getFilteredRowModel: getFilteredRowModel(), // Client-side filtering if needed
        onSortingChange: setSorting,
        onRowSelectionChange: setRowSelection,
        state: {
            sorting,
            rowSelection,
        },
        getRowId: row => row.id, // Use GUID as Row ID
    });

    return (
        <div className="space-y-4">
            {Object.keys(rowSelection).length > 0 && (
                <div className="flex items-center gap-2 bg-muted/50 p-2 rounded-md">
                    <span className="text-sm text-muted-foreground">{Object.keys(rowSelection).length} selected</span>
                    <Button size="sm" onClick={handleBatchApprove}>
                        {t('common.approveSelected')}
                    </Button>
                </div>
            )}

            <div className="rounded-md border">
                <table className="w-full text-sm text-left">
                    <thead className="bg-muted/50 text-muted-foreground font-medium border-b">
                        {table.getHeaderGroups().map(headerGroup => (
                            <tr key={headerGroup.id}>
                                {headerGroup.headers.map(header => (
                                    <th key={header.id} className="h-12 px-4 align-middle font-medium text-muted-foreground">
                                        {header.isPlaceholder
                                            ? null
                                            : flexRender(
                                                header.column.columnDef.header,
                                                header.getContext()
                                            )}
                                    </th>
                                ))}
                            </tr>
                        ))}
                    </thead>
                    <tbody>
                        {table.getRowModel().rows?.length ? (
                            table.getRowModel().rows.map(row => (
                                <tr
                                    key={row.id}
                                    data-state={row.getIsSelected() && "selected"}
                                    className="border-b transition-colors hover:bg-muted/50 data-[state=selected]:bg-muted"
                                >
                                    {row.getVisibleCells().map(cell => (
                                        <td key={cell.id} className="p-4 align-middle">
                                            {flexRender(cell.column.columnDef.cell, cell.getContext())}
                                        </td>
                                    ))}
                                </tr>
                            ))
                        ) : (
                            <tr>
                                <td colSpan={columns.length} className="h-24 text-center">
                                    {t('common.noData')}
                                </td>
                            </tr>
                        )}
                    </tbody>
                </table>
            </div>
            <div className="flex items-center justify-end space-x-2 py-4">
                <Button
                    variant="outline"
                    size="sm"
                    onClick={() => table.previousPage()}
                    disabled={!table.getCanPreviousPage()}
                >
                    {t('common.previous')}
                </Button>
                <Button
                    variant="outline"
                    size="sm"
                    onClick={() => table.nextPage()}
                    disabled={!table.getCanNextPage()}
                >
                    {t('common.next')}
                </Button>
            </div>
        </div>
    );
}
