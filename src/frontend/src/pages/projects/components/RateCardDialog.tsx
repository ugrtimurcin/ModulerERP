import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { Button, Input, Select } from '@/components/ui';
import { MultiSelect } from '@/components/ui/MultiSelect';
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
type Currency = { id: string; code: string; symbol: string; name: string };

export function RateCardDialog({ isOpen, onClose, onSave, rateCard, projectId }: RateCardDialogProps) {
    const { t } = useTranslation();
    const toast = useToast();
    const [loading, setLoading] = useState(false);
    const [loadingLookups, setLoadingLookups] = useState(false);

    // Form State
    const [resourceType, setResourceType] = useState<'employee' | 'asset'>('employee');
    const [resourceIds, setResourceIds] = useState<string[]>([]);
    const [hourlyRate, setHourlyRate] = useState<number>(0);
    const [overtimeRate, setOvertimeRate] = useState<number>(0);
    const [currencyId, setCurrencyId] = useState('00000000-0000-0000-0000-000000000000'); // Default
    const [effectiveFrom, setEffectiveFrom] = useState(new Date().toISOString().split('T')[0]);
    const [effectiveTo, setEffectiveTo] = useState<string>('');

    // Lookups
    const [employees, setEmployees] = useState<LookupItem[]>([]);
    const [assets, setAssets] = useState<LookupItem[]>([]);
    const [currencies, setCurrencies] = useState<{ value: string; label: string }[]>([]);

    useEffect(() => {
        if (isOpen) {
            loadLookups();
            if (rateCard) {
                // Editing
                setResourceType(rateCard.employeeId ? 'employee' : 'asset');
                setResourceIds([rateCard.employeeId || rateCard.assetId || '']);
                setHourlyRate(rateCard.hourlyRate);
                setOvertimeRate(rateCard.overtimeRate || 0);
                setCurrencyId(rateCard.currencyId);
                setEffectiveFrom(rateCard.effectiveFrom.split('T')[0]);
                setEffectiveTo(rateCard.effectiveTo ? rateCard.effectiveTo.split('T')[0] : '');
            } else {
                // Creating
                setResourceType('employee');
                setResourceIds([]);
                setHourlyRate(0);
                setOvertimeRate(0);
                setEffectiveFrom(new Date().toISOString().split('T')[0]);
                setEffectiveTo('');
            }
        }
    }, [isOpen, rateCard]);

    useEffect(() => {
        // Auto-select first currency if creation mode and currencies loaded
        if (!rateCard && currencies.length > 0 && (currencyId === '00000000-0000-0000-0000-000000000000' || !currencyId)) {
            setCurrencyId(currencies[0].value);
        }
    }, [currencies, rateCard, currencyId]);

    const loadLookups = async () => {
        setLoadingLookups(true);
        try {
            const [empRes, assetRes, currRes] = await Promise.all([
                api.get<{ id: string; firstName: string; lastName: string; position: string }[]>('/hr/employees/lookup'),
                api.get<{ id: string; name: string; serialNumber: string }[]>('/fixedassets/assets/lookup'),
                api.get<{ data: Currency[] }>('/currencies/active')
            ]);

            if (empRes) {
                const empData = Array.isArray(empRes) ? empRes : (empRes as any).data;
                if (Array.isArray(empData)) {
                    setEmployees(empData.map((e: any) => ({
                        id: e.id,
                        label: `${e.firstName} ${e.lastName} (${e.position || e.JobTitle || e.jobTitle})`
                    })));
                }
            }
            if (assetRes) {
                const assetData = Array.isArray(assetRes) ? assetRes : (assetRes as any).data;
                if (Array.isArray(assetData)) {
                    setAssets(assetData.map((a: any) => ({
                        id: a.id,
                        label: `${a.name} (${a.serialNumber || 'No SN'})`
                    })));
                }
            }
            // Handle Currencies response (CurrenciesController returns { success: true, data: [...] })
            const currData = currRes.data || [];
            if (Array.isArray(currData)) {
                setCurrencies(currData.map(c => ({
                    value: c.id,
                    label: `${c.code} (${c.symbol})`
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
        if (resourceIds.length === 0) {
            toast.error(t('common.error'), "Please select at least one resource");
            return;
        }
        if (hourlyRate <= 0) {
            toast.error(t('common.error'), "Hourly rate must be greater than 0");
            return;
        }

        setLoading(true);
        try {
            // If editing, we only have one ID
            if (rateCard) {
                const data: any = {
                    hourlyRate,
                    overtimeRate,
                    currencyId,
                    effectiveFrom: new Date(effectiveFrom).toISOString(),
                    effectiveTo: effectiveTo ? new Date(effectiveTo).toISOString() : undefined
                };
                await onSave(data as UpdateResourceRateCardDto);
            } else {
                // Creating - Batch
                // We need to call onSave multiple times or refactor onSave to handle batch. 
                // However, the prop expects single DTO. 
                // Ideally backend supports batch, but here we can just loop.
                const promises = resourceIds.map(id => {
                    const data: any = {
                        hourlyRate,
                        overtimeRate,
                        currencyId,
                        effectiveFrom: new Date(effectiveFrom).toISOString(),
                        effectiveTo: effectiveTo ? new Date(effectiveTo).toISOString() : undefined,
                        projectId
                    };
                    if (resourceType === 'employee') data.employeeId = id;
                    else data.assetId = id;
                    return onSave(data as CreateResourceRateCardDto);
                });

                await Promise.all(promises);
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
                                <input type="radio" name="rcType" value="employee" checked={resourceType === 'employee'} onChange={() => { setResourceType('employee'); setResourceIds([]); }} />
                                <span>Employee</span>
                            </label>
                            <label className="flex items-center space-x-2">
                                <input type="radio" name="rcType" value="asset" checked={resourceType === 'asset'} onChange={() => { setResourceType('asset'); setResourceIds([]); }} />
                                <span>Asset</span>
                            </label>
                        </div>

                        {loadingLookups ? (
                            <div>Loading...</div>
                        ) : (
                            <div className="space-y-2">
                                <MultiSelect
                                    label="Resource(s)"
                                    options={resourceType === 'employee' ? employees.map(e => ({ label: e.label, value: e.id })) : assets.map(a => ({ label: a.label, value: a.id }))}
                                    value={resourceIds}
                                    onChange={setResourceIds}
                                    placeholder="Select Resources..."
                                />
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

                <div className="grid grid-cols-2 gap-4">
                    <Input
                        label="Standard Rate (Hourly)"
                        type="number"
                        value={hourlyRate}
                        onChange={(e) => {
                            const val = parseFloat(e.target.value);
                            setHourlyRate(isNaN(val) ? 0 : val);
                        }}
                        required
                    />
                    <Input
                        label="Overtime Rate (Hourly)"
                        type="number"
                        value={overtimeRate}
                        onChange={(e) => {
                            const val = parseFloat(e.target.value);
                            setOvertimeRate(isNaN(val) ? 0 : val);
                        }}
                        required
                    />
                </div>

                <Select
                    label={t('common.currency')}
                    options={currencies}
                    value={currencyId}
                    onChange={(e) => setCurrencyId(e.target.value)}
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
