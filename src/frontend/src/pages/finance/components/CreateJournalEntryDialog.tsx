import React, { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Trash2 } from 'lucide-react';
import { Modal, Button, Input, useToast } from '@/components/ui';
import { api } from '../../../services/api';

interface CreateJournalEntryDialogProps {
    isOpen: boolean;
    onClose: () => void;
    onSuccess: () => void;
}

interface JournalEntryLineDto {
    accountId: string;
    description: string;
    txDebit: number;
    txCredit: number;
    baseDebit: number;
    baseCredit: number;
    baseCurrencyId: string;
    transactionCurrencyId: string;
    exchangeRate: number;
    partnerId?: string;
}

export const CreateJournalEntryDialog: React.FC<CreateJournalEntryDialogProps> = ({
    isOpen,
    onClose,
    onSuccess
}) => {
    const { t } = useTranslation();
    const toast = useToast();
    const [loading, setLoading] = useState(false);

    // Header Data
    const [entryDate, setEntryDate] = useState(new Date().toISOString().split('T')[0]);
    const [description, setDescription] = useState('');
    const [referenceNumber, setReferenceNumber] = useState('');

    // Lines Data
    const [lines, setLines] = useState<JournalEntryLineDto[]>([]);

    // Metadata
    const [accounts, setAccounts] = useState<any[]>([]);
    const [currencies, setCurrencies] = useState<any[]>([]);
    const [baseCurrencyId, setBaseCurrencyId] = useState<string>('');

    useEffect(() => {
        if (isOpen) {
            loadMetadata();
            resetForm();
        }
    }, [isOpen]);

    const loadMetadata = async () => {
        try {
            const [accRes, currRes] = await Promise.all([
                api.finance.accounts.getAll(),
                api.getActiveCurrencies()
            ]);

            if (accRes.success && accRes.data) setAccounts(accRes.data);
            if (currRes.success && currRes.data) {
                setCurrencies(currRes.data);
                const tryCurrency = currRes.data.find(c => c.code === 'TRY') || currRes.data[0];
                if (tryCurrency) setBaseCurrencyId(tryCurrency.id);
            }
        } catch (error) {
            console.error(error);
            toast.error(t('common.error'), t('common.loadFailed'));
        }
    };

    const resetForm = () => {
        setEntryDate(new Date().toISOString().split('T')[0]);
        setDescription('');
        setReferenceNumber('');
        setLines([
            { accountId: '', description: '', txDebit: 0, txCredit: 0, baseDebit: 0, baseCredit: 0, baseCurrencyId: baseCurrencyId, transactionCurrencyId: baseCurrencyId, exchangeRate: 1 },
            { accountId: '', description: '', txDebit: 0, txCredit: 0, baseDebit: 0, baseCredit: 0, baseCurrencyId: baseCurrencyId, transactionCurrencyId: baseCurrencyId, exchangeRate: 1 }
        ]);
    };

    const handleAddLine = () => {
        setLines([...lines, { accountId: '', description: '', txDebit: 0, txCredit: 0, baseDebit: 0, baseCredit: 0, baseCurrencyId: baseCurrencyId, transactionCurrencyId: baseCurrencyId, exchangeRate: 1 }]);
    };

    const handleRemoveLine = (index: number) => {
        if (lines.length <= 2) {
            toast.warning(t('common.warning'), t('finance.journalEntries.minLines'));
            return;
        }
        const newLines = [...lines];
        newLines.splice(index, 1);
        setLines(newLines);
    };

    const updateLine = (index: number, field: keyof JournalEntryLineDto, value: any) => {
        const newLines = [...lines];
        newLines[index] = { ...newLines[index], [field]: value };
        setLines(newLines);
    };

    const calculateTotals = () => {
        const totalDebit = lines.reduce((sum, line) => sum + (Number(line.baseDebit) || 0), 0);
        const totalCredit = lines.reduce((sum, line) => sum + (Number(line.baseCredit) || 0), 0);
        return { totalDebit, totalCredit, difference: totalDebit - totalCredit };
    };

    const handleSave = async () => {
        const { totalDebit, difference } = calculateTotals();

        if (Math.abs(difference) > 0.001) {
            toast.error(t('common.error'), t('finance.journalEntries.balancedError', { amount: difference.toFixed(2) }));
            return;
        }

        if (totalDebit === 0) {
            toast.error(t('common.error'), t('finance.journalEntries.zeroError'));
            return;
        }

        const validLines = lines.filter(l => l.accountId && (l.baseDebit > 0 || l.baseCredit > 0 || l.txDebit > 0 || l.txCredit > 0));
        if (validLines.length < 2) {
            toast.error(t('common.error'), t('finance.journalEntries.minLines'));
            return;
        }

        setLoading(true);
        try {
            const dto = {
                entryDate,
                description,
                referenceNumber,
                lines: validLines.map(l => ({
                    accountId: l.accountId,
                    description: l.description || description,
                    txDebit: Number(l.txDebit),
                    txCredit: Number(l.txCredit),
                    baseDebit: Number(l.baseDebit),
                    baseCredit: Number(l.baseCredit),
                    baseCurrencyId: l.baseCurrencyId || baseCurrencyId,
                    transactionCurrencyId: l.transactionCurrencyId || baseCurrencyId,
                    exchangeRate: Number(l.exchangeRate) || 1,
                    partnerId: l.partnerId || null
                }))
            };

            const res = await api.finance.journalEntries.create(dto);
            if (res.success) {
                toast.success(t('common.success'), t('finance.journalEntries.entryCreated'));
                onSuccess();
                onClose();
            } else {
                toast.error(t('common.error'), res.error || t('finance.journalEntries.createFailed'));
            }
        } catch (error) {
            toast.error(t('common.error'), t('common.error'));
        } finally {
            setLoading(false);
        }
    };

    const totals = calculateTotals();
    const isBalanced = Math.abs(totals.difference) < 0.001;

    return (
        <Modal
            isOpen={isOpen}
            onClose={onClose}
            title={t('finance.journalEntries.createTitle')}
            size="xl"
            footer={
                <div className="flex justify-between w-full">
                    <div className={`text-sm font-bold flex items-center ${isBalanced ? 'text-green-600' : 'text-red-600'}`}>
                        {isBalanced ? t('finance.journalEntries.balanced') : t('finance.journalEntries.unbalanced', { amount: totals.difference.toFixed(2) })}
                    </div>
                    <div className="flex gap-2">
                        <Button variant="secondary" onClick={onClose}>{t('common.cancel')}</Button>
                        <Button onClick={handleSave} isLoading={loading} disabled={!isBalanced}>{t('common.save')}</Button>
                    </div>
                </div>
            }
        >
            <div className="space-y-4">
                {/* Header */}
                <div className="grid grid-cols-1 md:grid-cols-3 gap-4 p-4 bg-gray-50 rounded-lg">
                    <Input
                        label={t('finance.journalEntries.entryDate')}
                        type="date"
                        value={entryDate}
                        onChange={(e) => setEntryDate(e.target.value)}
                        required
                    />
                    <Input
                        label={t('finance.journalEntries.referenceNumber')}
                        value={referenceNumber}
                        onChange={(e) => setReferenceNumber(e.target.value)}
                        placeholder="Ref #"
                    />
                    <div className="md:col-span-3">
                        <Input
                            label={t('finance.journalEntries.description')}
                            value={description}
                            onChange={(e) => setDescription(e.target.value)}
                            placeholder={t('finance.journalEntries.description')}
                            required
                        />
                    </div>
                </div>

                {/* Lines */}
                <div className="space-y-2">
                    <div className="flex justify-between items-center">
                        <h3 className="font-semibold text-gray-700">{t('finance.journalEntries.lines')}</h3>
                        <Button size="sm" variant="secondary" onClick={handleAddLine}>
                            <Plus className="w-4 h-4 mr-2" />
                            {t('finance.journalEntries.addLine')}
                        </Button>
                    </div>

                    <div className="border rounded-md overflow-hidden">
                        <table className="min-w-full divide-y divide-gray-200">
                            <thead className="bg-gray-100">
                                <tr>
                                    <th className="px-3 py-2 text-left text-xs font-medium text-gray-500 uppercase w-48">{t('finance.journalEntries.account')}</th>
                                    <th className="px-3 py-2 text-left text-xs font-medium text-gray-500 uppercase w-32">Tx CCY & Rate</th>
                                    <th className="px-3 py-2 text-right text-xs font-medium text-gray-500 uppercase w-20">{t('finance.journalEntries.txDebit')}</th>
                                    <th className="px-3 py-2 text-right text-xs font-medium text-gray-500 uppercase w-20">{t('finance.journalEntries.txCredit')}</th>
                                    <th className="px-3 py-2 text-right text-xs font-medium text-gray-800 bg-gray-200 uppercase w-20">{t('finance.journalEntries.baseDebit')}</th>
                                    <th className="px-3 py-2 text-right text-xs font-medium text-gray-800 bg-gray-200 uppercase w-20">{t('finance.journalEntries.baseCredit')}</th>
                                    <th className="px-3 py-2 w-10"></th>
                                </tr>
                            </thead>
                            <tbody className="bg-white divide-y divide-gray-200">
                                {lines.map((line, index) => (
                                    <tr key={index}>
                                        <td className="px-2 py-2">
                                            <select
                                                className="w-full text-sm border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
                                                value={line.accountId}
                                                onChange={(e) => updateLine(index, 'accountId', e.target.value)}
                                            >
                                                <option value="">{t('finance.journalEntries.selectAccount')}</option>
                                                {accounts.map(acc => (
                                                    <option key={acc.id} value={acc.id}>
                                                        {acc.code} - {acc.name}
                                                    </option>
                                                ))}
                                            </select>
                                        </td>
                                        <td className="px-2 py-2">
                                            <div className="flex flex-col gap-1">
                                                <select
                                                    className="w-full text-xs border-gray-300 rounded-md"
                                                    value={line.transactionCurrencyId || ''}
                                                    onChange={(e) => updateLine(index, 'transactionCurrencyId', e.target.value)}
                                                >
                                                    {currencies.map(c => (
                                                        <option key={c.id} value={c.id}>{c.code}</option>
                                                    ))}
                                                </select>
                                                <input
                                                    type="number"
                                                    step="0.0001"
                                                    className="w-full text-xs text-right border-gray-300 rounded-md"
                                                    value={line.exchangeRate || 1}
                                                    onChange={(e) => {
                                                        const rate = parseFloat(e.target.value) || 1;
                                                        updateLine(index, 'exchangeRate', rate);
                                                        // Auto-calculate Base amounts if Tx amounts exist
                                                        if (line.txDebit > 0) updateLine(index, 'baseDebit', Number((line.txDebit * rate).toFixed(2)));
                                                        if (line.txCredit > 0) updateLine(index, 'baseCredit', Number((line.txCredit * rate).toFixed(2)));
                                                    }}
                                                    placeholder="Rate"
                                                />
                                            </div>
                                        </td>
                                        <td className="px-2 py-2">
                                            <input
                                                type="number"
                                                step="0.01"
                                                className="w-full text-sm text-right border-gray-300 rounded-md bg-green-50"
                                                value={line.txDebit || ''}
                                                onChange={(e) => {
                                                    const val = parseFloat(e.target.value) || 0;
                                                    updateLine(index, 'txDebit', val);
                                                    updateLine(index, 'baseDebit', Number((val * (line.exchangeRate || 1)).toFixed(2)));
                                                    if (val > 0) {
                                                        updateLine(index, 'txCredit', 0);
                                                        updateLine(index, 'baseCredit', 0);
                                                    }
                                                }}
                                            />
                                        </td>
                                        <td className="px-2 py-2">
                                            <input
                                                type="number"
                                                step="0.01"
                                                className="w-full text-sm text-right border-gray-300 rounded-md bg-green-50"
                                                value={line.txCredit || ''}
                                                onChange={(e) => {
                                                    const val = parseFloat(e.target.value) || 0;
                                                    updateLine(index, 'txCredit', val);
                                                    updateLine(index, 'baseCredit', Number((val * (line.exchangeRate || 1)).toFixed(2)));
                                                    if (val > 0) {
                                                        updateLine(index, 'txDebit', 0);
                                                        updateLine(index, 'baseDebit', 0);
                                                    }
                                                }}
                                            />
                                        </td>
                                        <td className="px-2 py-2 bg-gray-50">
                                            <input
                                                type="number"
                                                step="0.01"
                                                className="w-full text-sm text-right border-gray-400 rounded-md font-bold"
                                                value={line.baseDebit || ''}
                                                onChange={(e) => {
                                                    const val = parseFloat(e.target.value) || 0;
                                                    updateLine(index, 'baseDebit', val);
                                                    if (val > 0) updateLine(index, 'baseCredit', 0);
                                                }}
                                            />
                                        </td>
                                        <td className="px-2 py-2 bg-gray-50">
                                            <input
                                                type="number"
                                                step="0.01"
                                                className="w-full text-sm text-right border-gray-400 rounded-md font-bold"
                                                value={line.baseCredit || ''}
                                                onChange={(e) => {
                                                    const val = parseFloat(e.target.value) || 0;
                                                    updateLine(index, 'baseCredit', val);
                                                    if (val > 0) updateLine(index, 'baseDebit', 0);
                                                }}
                                            />
                                        </td>
                                        <td className="px-2 py-2 text-center">
                                            <button
                                                onClick={() => handleRemoveLine(index)}
                                                className="text-red-500 hover:text-red-700"
                                                tabIndex={-1}
                                            >
                                                <Trash2 className="w-4 h-4" />
                                            </button>
                                        </td>
                                    </tr>
                                ))}
                            </tbody>
                            <tfoot className="bg-gray-100 font-medium">
                                <tr>
                                    <td colSpan={2} className="px-3 py-2 text-right">{t('finance.journalEntries.totals')} (Base):</td>
                                    <td colSpan={2}></td>
                                    <td className="px-3 py-2 text-right text-blue-700 font-bold">{totals.totalDebit.toFixed(2)}</td>
                                    <td className="px-3 py-2 text-right text-blue-700 font-bold">{totals.totalCredit.toFixed(2)}</td>
                                    <td></td>
                                </tr>
                            </tfoot>
                        </table>
                    </div>
                </div>
            </div>
        </Modal>
    );
};
