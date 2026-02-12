import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { Button, Input } from '@/components/ui';
import { Modal } from '@/components/ui/Modal'; // Using older Modal style for consistency or new Dialog? Using Modal as per ResourcesTab
import { useToast } from '@/components/ui/Toast';
import { api } from '@/lib/api';
import type { CreateResourceRateCardDto, UpdateResourceRateCardDto, ResourceRateCardDto } from '@/types/project';

interface RateCardDialogProps {
    isOpen: boolean;
    onClose: () => void;
    onSave: (data: CreateResourceRateCardDto | UpdateResourceRateCardDto) => Promise<void>;
    rateCard?: ResourceRateCardDto;
    projectId?: string; // Optional: If set, we are creating a Project-Specific Rate Card
}

type LookupItem = { id: string; label: string };

export function RateCardDialog({ isOpen, onClose, onSave, rateCard, projectId }: RateCardDialogProps) {
    const { t } = useTranslation();
    const toast = useToast();
    const [loading, setLoading] = useState(false);
    const [loadingLookups, setLoadingLookups] = useState(false);

    // Form State
    const [resourceType, setResourceType] = useState<'employee' | 'asset'>('employee');
    const [resourceId, setResourceId] = useState('');
    const [hourlyRate, setHourlyRate] = useState<number>(0);
    const [currencyId, setCurrencyId] = useState('00000000-0000-0000-0000-000000000000'); // Default
    const [effectiveFrom, setEffectiveFrom] = useState(new Date().toISOString().split('T')[0]);
    const [effectiveTo, setEffectiveTo] = useState<string>('');

    // Lookups
    const [employees, setEmployees] = useState<LookupItem[]>([]);
    const [assets, setAssets] = useState<LookupItem[]>([]);

    useEffect(() => {
        if (isOpen) {
            loadLookups();
            if (rateCard) {
                // Editing
                setResourceType(rateCard.employeeId ? 'employee' : 'asset');
                setResourceId(rateCard.employeeId || rateCard.assetId || '');
                setHourlyRate(rateCard.hourlyRate);
                setCurrencyId(rateCard.currencyId);
                setEffectiveFrom(rateCard.effectiveFrom.split('T')[0]);
                setEffectiveTo(rateCard.effectiveTo ? rateCard.effectiveTo.split('T')[0] : '');
            } else {
                // Creating
                setResourceType('employee');
                setResourceId('');
                setHourlyRate(0);
                setEffectiveFrom(new Date().toISOString().split('T')[0]);
                setEffectiveTo('');
            }
        }
    }, [isOpen, rateCard]);

    const loadLookups = async () => {
        setLoadingLookups(true);
        try {
            const [empRes, assetRes] = await Promise.all([
                api.get<{ id: string; firstName: string; lastName: string; position: string }[]>('/hr/employees/lookup'),
                api.get<{ id: string; name: string; serialNumber: string }[]>('/fixedassets/assets/lookup')
            ]);

            if (empRes) {
                setEmployees(empRes.map((e) => ({
                    id: e.id,
                    label: `${e.firstName} ${e.lastName} (${e.position})`
                })));
            }
            if (assetRes) {
                setAssets(assetRes.map((a) => ({
                    id: a.id,
                    label: `${a.name} (${a.serialNumber || 'No SN'})`
                })));
            }
        } catch (error) {
            console.error("Failed to load lookups", error);
            toast.error(t('common.error'), "Failed to load resource options");
        } finally {
            setLoadingLookups(false);
        }
    };

    const handleSubmit = async () => {
        if (!resourceId) {
            toast.error(t('common.error'), "Please select a resource");
            return;
        }
        if (hourlyRate <= 0) {
            toast.error(t('common.error'), "Hourly rate must be greater than 0");
            return;
        }

        setLoading(true);
        try {
            const data: any = {
                hourlyRate,
                currencyId,
                effectiveFrom: new Date(effectiveFrom).toISOString(),
                effectiveTo: effectiveTo ? new Date(effectiveTo).toISOString() : undefined
            };

            if (!rateCard) {
                // Creating
                data.projectId = projectId; // Can be null for Global
                if (resourceType === 'employee') data.employeeId = resourceId;
                else data.assetId = resourceId;

                await onSave(data as CreateResourceRateCardDto);
            } else {
                // Updating
                await onSave(data as UpdateResourceRateCardDto);
            }
            onClose();
        } catch (error) {
            console.error("Save failed", error);
        } finally {
            setLoading(false);
        }
    };

    return (
        <Modal
            isOpen={isOpen}
            onClose={onClose}
            title={rateCard ? t('projects.rateCards.edit') : t('projects.rateCards.add')}
        >
            <div className="space-y-4 py-4">
                {/* Resource Type & Selection (Only when creating) */}
                {!rateCard && (
                    <>
                        <div className="flex space-x-4 mb-4">
                            <label className="flex items-center space-x-2">
                                <input type="radio" name="rcType" value="employee" checked={resourceType === 'employee'} onChange={() => { setResourceType('employee'); setResourceId(''); }} />
                                <span>Employee</span>
                            </label>
                            <label className="flex items-center space-x-2">
                                <input type="radio" name="rcType" value="asset" checked={resourceType === 'asset'} onChange={() => { setResourceType('asset'); setResourceId(''); }} />
                                <span>Asset</span>
                            </label>
                        </div>

                        {loadingLookups ? (
                            <div>Loading...</div>
                        ) : (
                            <div className="space-y-2">
                                <label className="text-sm font-medium">Resource</label>
                                <select
                                    className="flex h-10 w-full items-center justify-between rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
                                    value={resourceId}
                                    onChange={(e) => setResourceId(e.target.value)}
                                >
                                    <option value="">Select...</option>
                                    {(resourceType === 'employee' ? employees : assets).map(item => (
                                        <option key={item.id} value={item.id}>{item.label}</option>
                                    ))}
                                </select>
                            </div>
                        )}
                    </>
                )}

                {/* Read-only Resource Name if Editing */}
                {rateCard && (
                    <div className="text-sm text-muted-foreground mb-4">
                        Resource: {rateCard.employeeName || rateCard.assetName || "Unknown"}
                    </div>
                )}

                <Input
                    label={t('projects.hourlyCost')}
                    type="number"
                    value={hourlyRate}
                    onChange={(e) => setHourlyRate(parseFloat(e.target.value))}
                    required
                />

                <div className="grid grid-cols-2 gap-4">
                    <Input
                        label={t('common.effectiveFrom')}
                        type="date"
                        value={effectiveFrom}
                        onChange={(e) => setEffectiveFrom(e.target.value)}
                        required
                    />
                    <Input
                        label={t('common.effectiveTo')}
                        type="date"
                        value={effectiveTo}
                        onChange={(e) => setEffectiveTo(e.target.value)}
                    />
                </div>

                <div className="flex justify-end space-x-2 mt-6">
                    <Button variant="ghost" onClick={onClose} disabled={loading}>{t('common.cancel')}</Button>
                    <Button onClick={handleSubmit} disabled={loading}>
                        {loading ? t('common.saving') : t('common.save')}
                    </Button>
                </div>
            </div>
        </Modal>
    );
}
