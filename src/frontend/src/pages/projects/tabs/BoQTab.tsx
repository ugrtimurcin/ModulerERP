import React, { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Edit2, Trash2, ChevronRight, ChevronDown, FolderPlus } from 'lucide-react';
import { api } from '@/lib/api';
import type { BillOfQuantitiesItemDto, CreateBoQItemDto, UpdateBoQItemDto, ProjectDto } from '@/types/project';
import { BoQItemDialog } from '../components/BoQItemDialog';
import { useToast } from '@/components/ui/Toast';

interface BoQTabProps {
    projectId: string;
}

export function BoQTab({ projectId }: BoQTabProps) {
    const { t } = useTranslation();
    const toast = useToast();
    const [items, setItems] = useState<BillOfQuantitiesItemDto[]>([]);
    const [loading, setLoading] = useState(true);
    const [isDialogOpen, setIsDialogOpen] = useState(false);
    const [editingItem, setEditingItem] = useState<BillOfQuantitiesItemDto | undefined>(undefined);
    const [parentItem, setParentItem] = useState<BillOfQuantitiesItemDto | undefined>(undefined);

    // Expansion state
    const [expanded, setExpanded] = useState<Record<string, boolean>>({});

    useEffect(() => {
        loadData();
    }, [projectId]);

    const loadData = async () => {
        setLoading(true);
        try {
            // Note: BoQ Items are currently part of Project Aggregate
            const response = await api.get<{ data: ProjectDto }>(`/projects/${projectId}`);
            if (response.data) {
                // Ensure boQItems is populated. If backend returns null, default to empty
                setItems(response.data.boQItems || []);
            }
        } catch (error) {
            console.error(error);
            toast.error(t('common.errorLoading'));
        } finally {
            setLoading(false);
        }
    };

    const handleAddTopLevel = () => {
        setEditingItem(undefined);
        setParentItem(undefined);
        setIsDialogOpen(true);
    };

    const handleAddChild = (parent: BillOfQuantitiesItemDto) => {
        setEditingItem(undefined);
        setParentItem(parent);
        setIsDialogOpen(true);
    };

    const handleEdit = (item: BillOfQuantitiesItemDto) => {
        setEditingItem(item);
        setParentItem(undefined); // Parent ID is inside item if needed
        setIsDialogOpen(true);
    };

    const handleDelete = async (item: BillOfQuantitiesItemDto) => {
        if (!window.confirm(t('common.confirmDelete'))) return;

        try {
            await api.delete(`/projects/${projectId}/boq-items/${item.id}`);
            toast.success(t('common.deleted'));
            loadData();
        } catch (error) {
            console.error(error);
            toast.error(t('common.errorDeleting'));
        }
    };

    const toggleExpand = (id: string) => {
        setExpanded(prev => ({ ...prev, [id]: !prev[id] }));
    };

    const handleSubmit = async (data: CreateBoQItemDto | UpdateBoQItemDto) => {
        try {
            if (editingItem) {
                await api.put(`/projects/${projectId}/boq-items/${editingItem.id}`, data);
            } else {
                await api.post(`/projects/${projectId}/boq-items`, data);
            }

            toast.success(t('common.saved'));
            setIsDialogOpen(false);
            loadData();
        } catch (error) {
            console.error(error);
            toast.error(t('common.errorSaving'));
        }
    };

    // Render Logic
    const renderRow = (item: BillOfQuantitiesItemDto, level: number) => {
        const hasChildren = item.children && item.children.length > 0;
        const isExpanded = expanded[item.id] !== false; // Default expanded

        return (
            <React.Fragment key={item.id}>
                <tr className="hover:bg-gray-50 border-b">
                    <td className="px-4 py-2">
                        <div className="flex items-center" style={{ paddingLeft: `${level * 20}px` }}>
                            {hasChildren && (
                                <button onClick={() => toggleExpand(item.id)} className="mr-1">
                                    {isExpanded ? <ChevronDown size={16} /> : <ChevronRight size={16} />}
                                </button>
                            )}
                            {!hasChildren && <span className="w-4 mr-1"></span>}
                            <span className="font-mono text-sm">{item.itemCode}</span>
                        </div>
                    </td>
                    <td className="px-4 py-2 text-sm">{item.description}</td>
                    <td className="px-4 py-2 text-sm text-right">{item.quantity}</td>
                    <td className="px-4 py-2 text-sm text-center">
                        {/* Unit Name placeholder */}
                        -
                    </td>
                    <td className="px-4 py-2 text-sm text-right ">
                        <div className="text-green-600 font-medium">{item.contractUnitPrice > 0 ? item.contractUnitPrice.toFixed(2) : '-'}</div>
                        <div className="text-xs text-gray-500">{item.totalContractAmount > 0 ? item.totalContractAmount.toFixed(2) : '-'}</div>
                    </td>
                    <td className="px-4 py-2 text-sm text-right">
                        <div className="text-red-600 font-medium">{item.estimatedUnitCost > 0 ? item.estimatedUnitCost.toFixed(2) : '-'}</div>
                        <div className="text-xs text-gray-500">{item.totalEstimatedCost > 0 ? item.totalEstimatedCost.toFixed(2) : '-'}</div>
                    </td>
                    <td className="px-4 py-2 text-right">
                        <div className="flex justify-end space-x-2">
                            <button onClick={() => handleAddChild(item)} title={t('common.addSubItem')} className="text-blue-600 hover:text-blue-800">
                                <FolderPlus size={16} />
                            </button>
                            <button onClick={() => handleEdit(item)} title={t('common.edit')} className="text-gray-600 hover:text-gray-800">
                                <Edit2 size={16} />
                            </button>
                            <button onClick={() => handleDelete(item)} title={t('common.delete')} className="text-red-600 hover:text-red-800">
                                <Trash2 size={16} />
                            </button>
                        </div>
                    </td>
                </tr>
                {hasChildren && isExpanded && item.children!.map(child => renderRow(child, level + 1))}
            </React.Fragment>
        );
    };

    if (loading) return <div>{t('common.loading')}</div>;

    return (
        <div className="space-y-4">
            <div className="flex justify-between items-center">
                <h3 className="text-lg font-medium">{t('projects.boq.title')}</h3>
                <button
                    onClick={handleAddTopLevel}
                    className="flex items-center px-4 py-2 bg-indigo-600 text-white rounded-md hover:bg-indigo-700"
                >
                    <Plus size={16} className="mr-2" />
                    {t('projects.boq.addItem')}
                </button>
            </div>

            <div className="overflow-x-auto bg-white rounded-lg shadow border">
                <table className="min-w-full divide-y divide-gray-200">
                    <thead className="bg-gray-50">
                        <tr>
                            <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">{t('projects.boq.itemCode')}</th>
                            <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">{t('common.description')}</th>
                            <th className="px-4 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">{t('common.quantity')}</th>
                            <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider">{t('common.unit')}</th>
                            <th className="px-4 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">{t('projects.boq.income')}</th>
                            <th className="px-4 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">{t('projects.boq.expense')}</th>
                            <th className="px-4 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">{t('common.actions')}</th>
                        </tr>
                    </thead>
                    <tbody className="bg-white divide-y divide-gray-200">
                        {items.length === 0 ? (
                            <tr>
                                <td colSpan={7} className="px-6 py-4 text-center text-gray-500">
                                    {t('common.noData')}
                                </td>
                            </tr>
                        ) : (
                            items.map(item => renderRow(item, 0))
                        )}
                    </tbody>
                </table>
            </div>

            <BoQItemDialog
                isOpen={isDialogOpen}
                onClose={() => setIsDialogOpen(false)}
                onSubmit={handleSubmit}
                initialData={editingItem}
                parentId={parentItem?.id}
                projectId={projectId}
            />
        </div>
    );
}
