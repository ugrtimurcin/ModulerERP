import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { Globe } from 'lucide-react';
import { api } from '@/services/api';
import { DataTable, Badge } from '@/components/ui';
import type { Column } from '@/components/ui';

interface Language {
    id: string;
    code: string;
    name: string;
    isRtl: boolean;
    isActive: boolean;
}

export function LanguagesPage() {
    const { t } = useTranslation();

    const [languages, setLanguages] = useState<Language[]>([]);
    const [isLoading, setIsLoading] = useState(true);

    useEffect(() => {
        async function load() {
            const result = await api.getLanguages();
            if (result.success && result.data) {
                setLanguages(result.data);
            }
            setIsLoading(false);
        }
        load();
    }, []);

    const columns: Column<Language>[] = [
        {
            key: 'code',
            header: t('languages.code'),
            render: (language) => (
                <div className="flex items-center gap-3">
                    <div className="w-10 h-10 rounded-lg bg-blue-100 dark:bg-blue-900/30 text-blue-600 flex items-center justify-center">
                        <Globe className="w-5 h-5" />
                    </div>
                    <div>
                        <p className="font-medium">{language.name}</p>
                        <p className="text-sm text-[hsl(var(--muted-foreground))]">
                            {language.code}
                        </p>
                    </div>
                </div>
            ),
        },
        {
            key: 'isRtl',
            header: t('languages.isRtl'),
            render: (language) => (
                <Badge variant={language.isRtl ? 'info' : 'default'}>
                    {language.isRtl ? t('common.yes') : t('common.no')}
                </Badge>
            ),
        },
        {
            key: 'isActive',
            header: t('common.status'),
            render: (language) => (
                <Badge variant={language.isActive ? 'success' : 'error'}>
                    {language.isActive ? t('common.active') : t('common.inactive')}
                </Badge>
            ),
        },
    ];

    return (
        <div className="space-y-6">
            {/* Header */}
            <div>
                <h1 className="text-2xl font-bold">{t('languages.title')}</h1>
                <p className="text-[hsl(var(--muted-foreground))]">
                    {t('languages.subtitle')}
                </p>
            </div>

            {/* Table */}
            <DataTable
                data={languages}
                columns={columns}
                keyField="id"
                isLoading={isLoading}
            />
        </div>
    );
}
