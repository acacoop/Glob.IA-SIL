import { render, screen } from '@testing-library/react';
import AddContact from './AddContact';
import { BrowserRouter } from 'react-router-dom';

test('renders page', () => {
  render(<BrowserRouter><AddContact /></BrowserRouter>);
  const title = screen.getByText(/Nuevo Contacto/i);
  expect(title).toBeInTheDocument();
});