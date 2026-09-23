import { fireEvent, render, screen } from '@testing-library/react';
import FormContact from './FormContact';
import { BrowserRouter } from 'react-router-dom';

test('renders page', () => {
  render(<BrowserRouter><FormContact /></BrowserRouter>);
  const label = screen.getByLabelText(/Nombre/i);
  expect(label).toBeInTheDocument();
});

test('can open accordion of contacts', () => {
  render(<BrowserRouter><FormContact /></BrowserRouter>);
  const button = screen.getByText(/Agregar Contacto/i);
  expect(button).toBeInTheDocument();
  fireEvent.click(button);
  const newContact = screen.getAllByText(/Tipo Contacto/);
  expect(newContact).toBeVisible();
});