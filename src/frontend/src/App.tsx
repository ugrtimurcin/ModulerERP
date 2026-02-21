import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useAuthStore } from '@/stores';
import { MainLayout } from '@/layouts/MainLayout';
import { AuthLayout } from '@/layouts/AuthLayout';
import { LoginPage } from '@/pages/LoginPage';
import { DashboardPage } from '@/pages/DashboardPage';
import { UsersPage } from '@/pages/UsersPage';
import { RolesPage } from '@/pages/RolesPage';
import { CurrenciesPage } from '@/pages/CurrenciesPage';
import { LanguagesPage } from '@/pages/LanguagesPage';
import { PartnersPage } from '@/pages/PartnersPage';
import { BusinessPartnerDetail } from '@/pages/crm/BusinessPartnerDetail';
import { ContactsPage } from '@/pages/ContactsPage';
import { LeadsPage } from '@/pages/LeadsPage';
import { LeadDetail } from '@/pages/crm/LeadDetail';
import { OpportunitiesPage } from '@/pages/OpportunitiesPage';
import { OpportunityDetail } from '@/pages/crm/OpportunityDetail';
import { TagsPage } from '@/pages/TagsPage';
import { TicketsPage } from '@/pages/TicketsPage';
import { CrmSettingsPage } from '@/pages/crm/settings/CrmSettingsPage';
import { ProductCategoriesPage } from '@/pages/inventory/ProductCategoriesPage';
import { WarehousesPage } from '@/pages/inventory/WarehousesPage';
import { UnitOfMeasuresPage } from '@/pages/inventory/UnitOfMeasuresPage';
import { ProductsPage } from '@/pages/inventory/ProductsPage';
import { ToastContainer, Dialog } from '@/components/ui';
import { ProductDetailPage } from './pages/inventory/ProductDetailPage';
import StockLevelsPage from './pages/inventory/StockLevelsPage';
import StockTransferWizard from './pages/inventory/StockTransferWizard';
import StockMovementsPage from './pages/inventory/StockMovementsPage';
import QuotesPage from './pages/sales/QuotesPage';
import OrdersPage from './pages/sales/OrdersPage';
import InvoicesPage from './pages/sales/InvoicesPage';
import { ShipmentsPage } from './pages/sales/ShipmentsPage';
import JournalEntriesPage from './pages/finance/JournalEntriesPage';
import AccountsPage from './pages/finance/AccountsPage';
import PaymentsPage from './pages/finance/PaymentsPage';
import FiscalPeriodsPage from './pages/finance/FiscalPeriodsPage';
import ExchangeRatesPage from './pages/finance/ExchangeRatesPage';
import { ChequesPage } from '@/pages/finance/ChequesPage';
import { EmployeesPage } from '@/pages/hr/EmployeesPage';
import { DepartmentsPage } from '@/pages/hr/DepartmentsPage';
import { AttendancePage } from '@/pages/hr/AttendancePage';
import { LeaveRequestsPage } from '@/pages/hr/LeaveRequestsPage';
import { PayrollPage } from '@/pages/hr/PayrollPage';
import { WorkShiftsPage } from '@/pages/hr/WorkShiftsPage';
import { AdvanceRequestsPage } from '@/pages/hr/AdvanceRequestsPage';
import { PurchaseOrdersPage } from '@/pages/procurement/PurchaseOrdersPage';
import { GoodsReceiptsPage } from '@/pages/procurement/GoodsReceiptsPage';
import { RFQsPage } from '@/pages/procurement/RFQsPage';
import { PurchaseQuotesPage } from '@/pages/procurement/PurchaseQuotesPage';
import { QualityControlPage } from '@/pages/procurement/QualityControlPage';
import { PurchaseReturnsPage } from '@/pages/procurement/PurchaseReturnsPage';
import { PublicHolidaysPage } from '@/pages/hr/PublicHolidaysPage';
import { CommissionRulesPage } from '@/pages/hr/CommissionRulesPage';
import { AttendanceLogsPage } from '@/pages/hr/AttendanceLogsPage';
import { HRSettingsPage } from '@/pages/hr/HRSettingsPage';
import { AssetsPage } from '@/pages/fixedassets/AssetsPage';
import { AssetCategoriesPage } from '@/pages/fixedassets/AssetCategoriesPage';
import { AssetDetailPage } from '@/pages/fixedassets/AssetDetailPage';
import { BomPage } from '@/pages/BomPage';
import { ProductionOrdersPage } from '@/pages/ProductionOrdersPage';
import { BrandsPage } from '@/pages/BrandsPage';
import { ProjectsPage } from '@/pages/projects/ProjectsPage';
import { ProjectDetailPage } from '@/pages/projects/ProjectDetailPage';

// Protected Route wrapper
function ProtectedRoute({ children }: { children: React.ReactNode }) {
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated);

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  return <>{children}</>;
}

function App() {
  return (
    <BrowserRouter>
      {/* Global UI Components */}
      <ToastContainer />
      <Dialog />

      <Routes>
        {/* Auth routes */}
        <Route element={<AuthLayout />}>
          <Route path="/login" element={<LoginPage />} />
        </Route>

        {/* Protected routes */}
        <Route
          element={
            <ProtectedRoute>
              <MainLayout />
            </ProtectedRoute>
          }
        >
          <Route path="/dashboard" element={<DashboardPage />} />

          {/* System Core */}
          <Route path="/users" element={<UsersPage />} />
          <Route path="/roles" element={<RolesPage />} />
          <Route path="/currencies" element={<CurrenciesPage />} />
          <Route path="/languages" element={<LanguagesPage />} />

          {/* CRM */}
          <Route path="/partners" element={<PartnersPage />} />
          <Route path="/partners/new" element={<BusinessPartnerDetail mode="create" />} />
          <Route path="/partners/:id" element={<BusinessPartnerDetail mode="edit" />} />
          <Route path="/contacts" element={<ContactsPage />} />
          <Route path="/leads" element={<LeadsPage />} />
          <Route path="/leads/new" element={<LeadDetail mode="create" />} />
          <Route path="/leads/:id" element={<LeadDetail mode="edit" />} />
          <Route path="/opportunities" element={<OpportunitiesPage />} />
          <Route path="/opportunities/new" element={<OpportunityDetail mode="create" />} />
          <Route path="/opportunities/:id" element={<OpportunityDetail mode="edit" />} />
          <Route path="/tags" element={<TagsPage />} />
          <Route path="/tickets" element={<TicketsPage />} />
          <Route path="/crm/settings" element={<CrmSettingsPage />} />
          <Route path="/inventory/categories" element={<ProductCategoriesPage />} />
          <Route path="/inventory/warehouses" element={<WarehousesPage />} />
          <Route path="/inventory/uom" element={<UnitOfMeasuresPage />} />
          <Route path="/inventory/products" element={<ProductsPage />} />
          <Route path="/inventory/products/new" element={<ProductDetailPage mode="create" />} />
          <Route path="/inventory/products/:id" element={<ProductDetailPage mode="edit" />} />
          <Route path="/inventory/products/:id" element={<ProductDetailPage mode="edit" />} />
          <Route path="/inventory/products/:id" element={<ProductDetailPage mode="edit" />} />
          <Route path="/inventory/levels" element={<StockLevelsPage />} />
          <Route path="/inventory/transfers/new" element={<StockTransferWizard />} />
          <Route path="/inventory/movements" element={<StockMovementsPage />} />
          <Route path="/inventory/brands" element={<BrandsPage />} />
          <Route path="/inventory/*" element={<PlaceholderPage titleKey="nav.inventory" />} />

          <Route path="/sales/quotes" element={<QuotesPage />} />
          <Route path="/sales/orders" element={<OrdersPage />} />
          <Route path="/sales/invoices" element={<InvoicesPage />} />
          <Route path="/sales/shipments" element={<ShipmentsPage />} />
          <Route path="/sales/*" element={<PlaceholderPage titleKey="nav.sales" />} />

          {/* Procurement */}
          <Route path="/procurement/purchase-orders" element={<PurchaseOrdersPage />} />
          <Route path="/procurement/goods-receipts" element={<GoodsReceiptsPage />} />
          <Route path="/procurement/rfqs" element={<RFQsPage />} />
          <Route path="/procurement/purchase-quotes" element={<PurchaseQuotesPage />} />
          <Route path="/procurement/qc" element={<QualityControlPage />} />
          <Route path="/procurement/returns" element={<PurchaseReturnsPage />} />
          <Route path="/procurement/*" element={<PlaceholderPage titleKey="nav.procurement" />} />

          {/* Finance */}
          <Route path="/finance/accounts" element={<AccountsPage />} />
          <Route path="/finance/journal-entries" element={<JournalEntriesPage />} />
          <Route path="/finance/payments" element={<PaymentsPage />} />
          <Route path="/finance/rates" element={<ExchangeRatesPage />} />
          <Route path="/finance/fiscal-periods" element={<FiscalPeriodsPage />} />
          <Route path="/finance/cheques" element={<ChequesPage />} />
          <Route path="/finance/*" element={<PlaceholderPage titleKey="nav.finance" />} />

          {/* HR */}
          <Route path="/hr/employees" element={<EmployeesPage />} />
          <Route path="/hr/departments" element={<DepartmentsPage />} />
          <Route path="/hr/attendance" element={<AttendancePage />} />
          <Route path="/hr/leave-requests" element={<LeaveRequestsPage />} />
          <Route path="/hr/payroll" element={<PayrollPage />} />
          <Route path="/hr/work-shifts" element={<WorkShiftsPage />} />

          <Route path="/hr/advance-requests" element={<AdvanceRequestsPage />} />
          <Route path="/hr/public-holidays" element={<PublicHolidaysPage />} />
          <Route path="/hr/commission-rules" element={<CommissionRulesPage />} />
          <Route path="/hr/attendance-logs" element={<AttendanceLogsPage />} />
          <Route path="/hr/settings" element={<HRSettingsPage />} />
          <Route path="/hr/*" element={<PlaceholderPage titleKey="nav.hr" />} />

          {/* Fixed Assets */}
          <Route path="/fixed-assets/assets" element={<AssetsPage />} />
          <Route path="/fixed-assets/assets/:id" element={<AssetDetailPage />} />
          <Route path="/fixed-assets/categories" element={<AssetCategoriesPage />} />
          <Route path="/fixed-assets/*" element={<PlaceholderPage titleKey="nav.fixedAssets" />} />

          {/* Manufacturing */}
          <Route path="/manufacturing/bom" element={<BomPage />} />
          <Route path="/manufacturing/orders" element={<ProductionOrdersPage />} />
          <Route path="/manufacturing/*" element={<PlaceholderPage titleKey="nav.manufacturing" />} />

          {/* Project Management */}
          <Route path="/projects" element={<ProjectsPage />} />
          <Route path="/projects/new" element={<ProjectDetailPage mode="create" />} />
          <Route path="/projects/:id" element={<ProjectDetailPage mode="view" />} />
          <Route path="/projects/:id/edit" element={<ProjectDetailPage mode="edit" />} />

          <Route path="/reports/*" element={<PlaceholderPage titleKey="nav.reports" />} />
          <Route path="/settings/*" element={<PlaceholderPage titleKey="nav.settings" />} />
        </Route>

        {/* Default redirect */}
        <Route path="/" element={<Navigate to="/dashboard" replace />} />
        <Route path="*" element={<Navigate to="/dashboard" replace />} />
      </Routes>
    </BrowserRouter>
  );
}

function PlaceholderPage({ titleKey }: { titleKey: string }) {
  const { t } = useTranslation();
  const title = t(titleKey);

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold">{title}</h1>
        <p className="text-[hsl(var(--muted-foreground))]">
          {t('common.underDevelopment')}
        </p>
      </div>
      <div className="bg-[hsl(var(--card))] rounded-xl border p-12 text-center">
        <div className="max-w-md mx-auto">
          <div className="w-16 h-16 rounded-full bg-indigo-100 dark:bg-indigo-900/20 text-indigo-600 flex items-center justify-center mx-auto mb-4">
            <svg className="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10" />
            </svg>
          </div>
          <h3 className="text-lg font-semibold mb-2">{t('common.comingSoon')}</h3>
          <p className="text-[hsl(var(--muted-foreground))]">
            {t('common.underDevelopment')}
          </p>
        </div>
      </div>
    </div>
  );
}

export default App;
