import { Badge } from '@/components/ui';
import { useTranslation } from 'react-i18next';

export const ChequeStatusBadge = ({ status }: { status: number }) => {
    const { t } = useTranslation();
    switch (status) {
        case 0: return <Badge variant="default">{t('cheques.statuses.portfolio', 'Portfolio')}</Badge>;
        case 1: return <Badge variant="warning">{t('cheques.statuses.endorsed', 'Endorsed')}</Badge>;
        case 2: return <Badge variant="warning">{t('cheques.statuses.bankCollection', 'Bank Coll.')}</Badge>;
        case 3: return <Badge variant="warning">{t('cheques.statuses.pledged', 'Pledged')}</Badge>;
        case 4: return <Badge variant="success">{t('cheques.statuses.paid', 'Paid')}</Badge>;
        case 5: return <Badge variant="error">{t('cheques.statuses.bounced', 'Bounced')}</Badge>;
        case 6: return <Badge variant="default">{t('cheques.statuses.returned', 'Returned')}</Badge>;
        default: return <Badge variant="default">{t('common.unknown', 'Unknown')}</Badge>;
    }
};

export const ChequeTypeBadge = ({ type }: { type: number }) => {
    const { t } = useTranslation();
    switch (type) {
        case 1: return <Badge variant="info">{t('cheques.types.own', 'Own')}</Badge>;
        case 2: return <Badge variant="default">{t('cheques.types.customer', 'Customer')}</Badge>;
        default: return <Badge>{t('common.unknown', 'Unknown')}</Badge>;
    }
};
