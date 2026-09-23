import { Routes, Route } from 'react-router-dom';

import { lazy } from 'react';

// routes
import config from 'config';

import MainLayout from 'layout/MainLayout';
import MinimalLayout from 'layout/MinimalLayout';
import Loadable from 'components/load/Loadable';
import Home from 'views/home'
import ProtectedRoute from './ProtectedRoute';

const Contacts = Loadable(lazy(() => import('views/contacts/Contacts')));
const ContactsList = Loadable(lazy(() => import('views/contactslist/ContactsList')));
const AddContact = Loadable(lazy(() => import('views/contacts/AddContact')));
const EditContact = Loadable(lazy(() => import('views/contacts/EditContact')));
const AddContactList = Loadable(lazy(() => import('views/contactslist/AddContactList')));
const EditContactList = Loadable(lazy(() => import('views/contactslist/EditContactList')));
const AuthLogin3 = Loadable(lazy(() => import('views/pages/authentication/authentication3/Login3')));
const SignIn = Loadable(lazy(() => import('views/pages/authentication/authentication3/SignIn')));

// ==============================|| ROUTING RENDER ||============================== //

export default function ThemeRoutes() {
    return (
        <Routes>
            <Route path="/" element={<ProtectedRoute><MainLayout /></ProtectedRoute>}>
                <Route index element={<Home />} />
                <Route path="contactos" element={<Contacts />} />
                <Route path="contactos/nuevo" element={<AddContact />} />
                <Route path="contactos/:id" element={<EditContact />} />
                <Route path="listas-contactos" element={<ContactsList />} />
                <Route path="listas-contactos/nuevo" element={<AddContactList />} />
                <Route path="listas-contactos/:id" element={<EditContactList />} />
            </Route>
            <Route path="/" element={<MinimalLayout />}>
                <Route path="login" element={<AuthLogin3 />} />
                <Route path="signin-callback" element={<SignIn />} />
            </Route>
        </Routes>
    )
}
