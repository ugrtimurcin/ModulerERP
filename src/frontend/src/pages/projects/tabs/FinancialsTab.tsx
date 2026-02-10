import { useTranslation } from 'react-i18next';

interface FinancialsTabProps {
    projectId: string;
}

export function FinancialsTab({ projectId: _projectId }: FinancialsTabProps) {
    const { t } = useTranslation();

    // TODO: Refactor to use BoQ data or new financial summary logic
    return (
        <div className="p-4 text-center text-muted-foreground">
            {t('common.featureUnderConstruction')}
        </div>
    );
}
