import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { ChevronLeft, ChevronRight, Search, ChevronsLeft, ChevronsRight } from 'lucide-react';

export interface Column<T> {
    key: string;
    header: string;
    render?: (item: T) => React.ReactNode;
    sortable?: boolean;
    width?: string;
    align?: 'left' | 'center' | 'right';
}

interface DataTableProps<T> {
    data: T[];
    columns: Column<T>[];
    keyField: keyof T;
    isLoading?: boolean;
    searchable?: boolean;
    searchPlaceholder?: string;
    onSearch?: (query: string) => void;
    pagination?: {
        page: number;
        pageSize: number;
        total: number;
        onPageChange: (page: number) => void;
    };
    actions?: (item: T) => React.ReactNode;
    emptyMessage?: string;
}

export function DataTable<T extends Record<string, any>>({
    data,
    columns,
    keyField,
    isLoading,
    searchable,
    searchPlaceholder,
    onSearch,
    pagination,
    actions,
    emptyMessage,
}: DataTableProps<T>) {
    const { t } = useTranslation();
    const [searchQuery, setSearchQuery] = useState('');

    const handleSearch = (e: React.ChangeEvent<HTMLInputElement>) => {
        const value = e.target.value;
        setSearchQuery(value);
        onSearch?.(value);
    };

    const totalPages = pagination
        ? Math.ceil(pagination.total / pagination.pageSize)
        : 1;

    return (
        <div className="bg-[hsl(var(--card))] rounded-xl border overflow-hidden">
            {/* Search Bar */}
            {searchable && (
                <div className="p-4 border-b border-[hsl(var(--border))]">
                    <div className="relative max-w-sm">
                        <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-[hsl(var(--muted-foreground))]" />
                        <input
                            type="text"
                            value={searchQuery}
                            onChange={handleSearch}
                            placeholder={searchPlaceholder || t('common.search')}
                            className="w-full pl-10 pr-4 py-2 rounded-lg border bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-indigo-500"
                        />
                    </div>
                </div>
            )}

            {/* Table */}
            <div className="overflow-x-auto">
                <table className="w-full">
                    <thead>
                        <tr className="bg-[hsl(var(--accent))]">
                            {columns.map((col) => (
                                <th
                                    key={col.key}
                                    className={`px-4 py-3 text-${col.align || 'left'} text-xs font-semibold uppercase tracking-wider`}
                                    style={{ width: col.width }}
                                >
                                    {col.header}
                                </th>
                            ))}
                            {actions && (
                                <th className="px-4 py-3 text-right text-xs font-semibold uppercase tracking-wider w-24">
                                    {t('common.actions')}
                                </th>
                            )}
                        </tr>
                    </thead>
                    <tbody className="divide-y divide-[hsl(var(--border))]">
                        {isLoading ? (
                            <tr>
                                <td
                                    colSpan={columns.length + (actions ? 1 : 0)}
                                    className="px-4 py-12 text-center"
                                >
                                    <div className="flex justify-center">
                                        <div className="animate-spin w-8 h-8 border-4 border-indigo-500 border-t-transparent rounded-full" />
                                    </div>
                                </td>
                            </tr>
                        ) : data.length === 0 ? (
                            <tr>
                                <td
                                    colSpan={columns.length + (actions ? 1 : 0)}
                                    className="px-4 py-12 text-center text-[hsl(var(--muted-foreground))]"
                                >
                                    {emptyMessage || t('common.noData')}
                                </td>
                            </tr>
                        ) : (
                            data.map((item) => (
                                <tr
                                    key={String(item[keyField])}
                                    className="hover:bg-[hsl(var(--accent)/0.5)] transition-colors"
                                >
                                    {columns.map((col) => (
                                        <td key={col.key} className={`px-4 py-3 text-sm text-${col.align || 'left'}`}>
                                            {col.render
                                                ? col.render(item)
                                                : String(item[col.key] ?? '')}
                                        </td>
                                    ))}
                                    {actions && (
                                        <td className="px-4 py-3 text-right">
                                            {actions(item)}
                                        </td>
                                    )}
                                </tr>
                            ))
                        )}
                    </tbody>
                </table>
            </div>

            {/* Pagination */}
            {pagination && totalPages > 1 && (
                <div className="flex items-center justify-between px-4 py-3 border-t border-[hsl(var(--border))]">
                    <p className="text-sm text-[hsl(var(--muted-foreground))]">
                        {t('common.showing')} {(pagination.page - 1) * pagination.pageSize + 1}-
                        {Math.min(pagination.page * pagination.pageSize, pagination.total)} {t('common.of')}{' '}
                        {pagination.total}
                    </p>
                    <div className="flex items-center gap-1">
                        <button
                            onClick={() => pagination.onPageChange(1)}
                            disabled={pagination.page === 1}
                            className="p-2 rounded-lg hover:bg-[hsl(var(--accent))] disabled:opacity-50 disabled:cursor-not-allowed"
                        >
                            <ChevronsLeft className="w-4 h-4" />
                        </button>
                        <button
                            onClick={() => pagination.onPageChange(pagination.page - 1)}
                            disabled={pagination.page === 1}
                            className="p-2 rounded-lg hover:bg-[hsl(var(--accent))] disabled:opacity-50 disabled:cursor-not-allowed"
                        >
                            <ChevronLeft className="w-4 h-4" />
                        </button>
                        <span className="px-3 py-1 text-sm">
                            {pagination.page} / {totalPages}
                        </span>
                        <button
                            onClick={() => pagination.onPageChange(pagination.page + 1)}
                            disabled={pagination.page >= totalPages}
                            className="p-2 rounded-lg hover:bg-[hsl(var(--accent))] disabled:opacity-50 disabled:cursor-not-allowed"
                        >
                            <ChevronRight className="w-4 h-4" />
                        </button>
                        <button
                            onClick={() => pagination.onPageChange(totalPages)}
                            disabled={pagination.page >= totalPages}
                            className="p-2 rounded-lg hover:bg-[hsl(var(--accent))] disabled:opacity-50 disabled:cursor-not-allowed"
                        >
                            <ChevronsRight className="w-4 h-4" />
                        </button>
                    </div>
                </div>
            )}
        </div>
    );
}
