import { Route, Routes } from 'react-router'
import { Roles } from './auth/roles.js'
import GuestOnly from './components/GuestOnly.jsx'
import Layout from './components/Layout.jsx'
import RequireAuth from './components/RequireAuth.jsx'
import HomePage from './pages/HomePage.jsx'
import ListingDetailPage from './pages/ListingDetailPage.jsx'
import ListingFormPage from './pages/ListingFormPage.jsx'
import ListingsPage from './pages/ListingsPage.jsx'
import LoginPage from './pages/LoginPage.jsx'
import MyListingsPage from './pages/MyListingsPage.jsx'
import NotFoundPage from './pages/NotFoundPage.jsx'
import RegisterPage from './pages/RegisterPage.jsx'

const LANDLORD_ROLES = [Roles.Landlord, Roles.Admin]

// Lehtede skeem: docs/disain.md
export default function App() {
  return (
    <Routes>
      <Route element={<Layout />}>
        <Route index element={<HomePage />} />
        <Route path="listings" element={<ListingsPage />} />
        <Route path="listings/:id" element={<ListingDetailPage />} />
        <Route
          path="my-listings"
          element={
            <RequireAuth roles={LANDLORD_ROLES}>
              <MyListingsPage />
            </RequireAuth>
          }
        />
        <Route
          path="my-listings/new"
          element={
            <RequireAuth roles={LANDLORD_ROLES}>
              <ListingFormPage />
            </RequireAuth>
          }
        />
        <Route
          path="my-listings/:id/edit"
          element={
            <RequireAuth roles={LANDLORD_ROLES}>
              <ListingFormPage />
            </RequireAuth>
          }
        />
        <Route
          path="login"
          element={
            <GuestOnly>
              <LoginPage />
            </GuestOnly>
          }
        />
        <Route
          path="register"
          element={
            <GuestOnly>
              <RegisterPage />
            </GuestOnly>
          }
        />
        <Route path="*" element={<NotFoundPage />} />
      </Route>
    </Routes>
  )
}
