import { render, screen } from '@testing-library/react';
import Contacts from './Contacts';
import { BrowserRouter } from 'react-router-dom';

test('renders page', () => {
  render(<BrowserRouter><Contacts /></BrowserRouter>);
  const title = screen.getByText(/Contactos/i);
  expect(title).toBeInTheDocument();
});