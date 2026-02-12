import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Trash, Edit, User, Truck } from 'lucide-react';
import { Button } from '@/components/ui';
import { useDialog } from '@/components/ui/Dialog';
import { useToast } from '@/components/ui/Toast';
import { api } from '@/lib/api';
import type { ResourceRateCardDto, CreateResourceRateCardDto, UpdateResourceRateCardDto } from '@/types/project';
import { RateCardDialog } from '../components/RateCardDialog';

interface RateCardsTabProps {
    projectId: string;
}

export function RateCardsTab({ projectId }: RateCardsTabProps) {
    const { t } = useTranslation();
    const { confirm } = useDialog();
    const toast = useToast();
    const [loading, setLoading] = useState(true);
    const [rateCards, setRateCards] = useState<ResourceRateCardDto[]>([]);
    const [isDialogOpen, setIsDialogOpen] = useState(false);
    const [editingCard, setEditingCard] = useState<ResourceRateCardDto | undefined>(undefined);

    useEffect(() => {
        loadRateCards();
    }, [projectId]);

    const loadRateCards = async () => {
        setLoading(true);
        try {
            const response = await api.get<{ data: ResourceRateCardDto[] }>(`/projects/rate-cards?projectId=${projectId}`);
            if (response.data) {
                setRateCards(response.data);
            }
        } catch (error) {
            console.error('Failed to load rate cards', error);
            toast.error(t('common.error'), "Failed to load rate cards");
        } finally {
            setLoading(false);
        }
    };

    const handleAddClick = () => {
        setEditingCard(undefined);
        setIsDialogOpen(true);
    };

    const handleEditClick = (card: ResourceRateCardDto) => {
        setEditingCard(card);
        setIsDialogOpen(true);
    };

    const handleDeleteClick = (card: ResourceRateCardDto) => {
        confirm({
            title: t('common.delete'),
            message: t('common.thisActionCannotBeUndone'),
            confirmText: t('common.delete'),
            onConfirm: async () => {
                try {
                    await api.delete(`/projects/rate-cards/${card.id}`);
                    toast.success(t('common.success'), t('common.deleted'));
                    loadRateCards();
                } catch (error) {
                    console.error('Failed to delete rate card', error);
                    toast.error(t('common.error'), t('common.errorDeleting'));
                }
            }
        });
    };

    const handleSave = async (data: CreateResourceRateCardDto | UpdateResourceRateCardDto) => {
        if (editingCard) {
            await api.put(`/projects/rate-cards/${editingCard.id}`, data);
            toast.success(t('common.success'), t('common.updated'));
        } else {
            // Force projectId for new cards created in this tab
            (data as CreateResourceRateCardDto).projectId = projectId;
            await api.post('/projects/rate-cards', data);
            toast.success(t('common.success'), t('common.saved'));
        }
        loadRateCards();
    };

    if (loading) return <div>{t('common.loading')}</div>;

    return (
        <div className="space-y-4">
            <div className="flex justify-between items-center">
                <h3 className="text-lg font-medium">{t('projects.tabs.rateCards')}</h3>
                <Button onClick={handleAddClick}>
                    <Plus className="mr-2 h-4 w-4" />
                    {t('projects.addRateCard')}
                </Button>
            </div>

            <div className="rounded-md border bg-card">
                <div className="p-0">
                    {rateCards.length === 0 ? (
                        <div className="text-center text-muted-foreground py-8">
                            {t('common.noData')}
                        </div>
                    ) : (
                        <div className="relative w-full overflow-auto">
                            <table className="w-full caption-bottom text-sm text-left">
                                <thead className="[&_tr]:border-b">
                                    <tr className="border-b transition-colors hover:bg-muted/50 data-[state=selected]:bg-muted">
                                        <th className="h-12 px-4 align-middle font-medium text-muted-foreground">{t('projects.tabs.rateCard.type')}</th>
                                        <th className="h-12 px-4 align-middle font-medium text-muted-foreground">{t('projects.tabs.rateCard.resource')}</th>
                                        <th className="h-12 px-4 align-middle font-medium text-muted-foreground">{t('projects.tabs.rateCard.rate')}</th>
                                        <th className="h-12 px-4 align-middle font-medium text-muted-foreground">{t('projects.tabs.rateCard.effectiveFrom')}</th>
                                        <th className="h-12 px-4 align-middle font-medium text-muted-foreground w-[100px]">{t('projects.tabs.rateCard.actions')}</th>
                                    </tr>
                                </thead>
                                <tbody className="[&_tr:last-child]:border-0">
                                    {rateCards.map(card => (
                                        <tr key={card.id} className="border-b transition-colors hover:bg-muted/50 data-[state=selected]:bg-muted">
                                            <td className="p-4 align-middle">
                                                {card.employeeId ? <User className="h-4 w-4" /> : <Truck className="h-4 w-4" />}
                                            </td>
                                            <td className="p-4 align-middle">
                                                {card.employeeName || card.assetName || '-'}
                                                {card.projectId ? <span className="ml-2 text-xs bg-blue-100 text-blue-800 px-1 rounded">{t('projects.tabs.rateCard.project')}</span> : <span className="ml-2 text-xs bg-gray-100 text-gray-800 px-1 rounded">{t('projects.tabs.rateCard.global')}</span>}
                                            </td>
                                            <td className="p-4 align-middle font-mono">
                                                {card.hourlyRate.toFixed(2)}
                                            </td>
                                            <td className="p-4 align-middle">
                                                {new Date(card.effectiveFrom).toLocaleDateString()}
                                                {card.effectiveTo ? ` - ${new Date(card.effectiveTo).toLocaleDateString()}` : ` - ${t('projects.tabs.rateCard.present')}`}
                                            </td>
                                            <td className="p-4 align-middle">
                                                <div className="flex items-center gap-2">
                                                    <Button variant="ghost" size="icon" className="h-8 w-8" onClick={() => handleEditClick(card)}>
                                                        <Edit className="h-4 w-4" />
                                                    </Button>
                                                    <Button variant="ghost" size="icon" className="h-8 w-8 text-red-500 hover:text-red-600" onClick={() => handleDeleteClick(card)}>
                                                        <Trash className="h-4 w-4" />
                                                    </Button>
                                                </div>
                                            </td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                        </div>
                    )}
                </div>
            </div>

            <RateCardDialog
                isOpen={isDialogOpen}
                onClose={() => setIsDialogOpen(false)}
                onSave={handleSave}
                rateCard={editingCard}
                projectId={projectId}
            />
        </div>
    );
}
