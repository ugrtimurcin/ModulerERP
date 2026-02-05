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
    debit: number;
    credit: number;
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

    useEffect(() => {
        if (isOpen) {
            loadMetadata();
            resetForm();
        }
    }, [isOpen]);

    const loadMetadata = async () => {
        try {
            const accRes = await api.finance.accounts.getAll();
            if (accRes.success && accRes.data) setAccounts(accRes.data);
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
            { accountId: '', description: '', debit: 0, credit: 0 },
            { accountId: '', description: '', debit: 0, credit: 0 }
        ]);
    };

    const handleAddLine = () => {
        setLines([...lines, { accountId: '', description: '', debit: 0, credit: 0 }]);
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
        const totalDebit = lines.reduce((sum, line) => sum + (Number(line.debit) || 0), 0);
        const totalCredit = lines.reduce((sum, line) => sum + (Number(line.credit) || 0), 0);
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

        const validLines = lines.filter(l => l.accountId && (l.debit > 0 || l.credit > 0));
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
                    debit: Number(l.debit),
                    credit: Number(l.credit),
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
                                    <th className="px-3 py-2 text-left text-xs font-medium text-gray-500 uppercase w-1/3">{t('finance.journalEntries.account')}</th>
                                    <th className="px-3 py-2 text-left text-xs font-medium text-gray-500 uppercase">{t('finance.journalEntries.lineDesc')}</th>
                                    <th className="px-3 py-2 text-right text-xs font-medium text-gray-500 uppercase w-24">{t('finance.journalEntries.debit')}</th>
                                    <th className="px-3 py-2 text-right text-xs font-medium text-gray-500 uppercase w-24">{t('finance.journalEntries.credit')}</th>
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
                                            <input
                                                type="text"
                                                className="w-full text-sm border-gray-300 rounded-md"
                                                value={line.description}
                                                onChange={(e) => updateLine(index, 'description', e.target.value)}
                                                placeholder={description}
                                            />
                                        </td>
                                        <td className="px-2 py-2">
                                            <input
                                                type="number"
                                                step="0.01"
                                                className="w-full text-sm text-right border-gray-300 rounded-md"
                                                value={line.debit}
                                                onChange={(e) => {
                                                    updateLine(index, 'debit', parseFloat(e.target.value));
                                                    if (parseFloat(e.target.value) > 0) updateLine(index, 'credit', 0);
                                                }}
                                            />
                                        </td>
                                        <td className="px-2 py-2">
                                            <input
                                                type="number"
                                                step="0.01"
                                                className="w-full text-sm text-right border-gray-300 rounded-md"
                                                value={line.credit}
                                                onChange={(e) => {
                                                    updateLine(index, 'credit', parseFloat(e.target.value));
                                                    if (parseFloat(e.target.value) > 0) updateLine(index, 'debit', 0);
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
                            <tfoot className="bg-gray-50 font-medium">
                                <tr>
                                    <td colSpan={2} className="px-3 py-2 text-right">{t('finance.journalEntries.totals')}:</td>
                                    <td className="px-3 py-2 text-right text-blue-700">{totals.totalDebit.toFixed(2)}</td>
                                    <td className="px-3 py-2 text-right text-blue-700">{totals.totalCredit.toFixed(2)}</td>
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
