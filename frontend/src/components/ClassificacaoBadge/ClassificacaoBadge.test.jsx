import React from 'react';
import { render, screen } from '@testing-library/react';
import { describe, it, expect } from 'vitest';
import ClassificacaoBadge, { obterInfoClassificacao } from './ClassificacaoBadge';

describe('ClassificacaoBadge Helper & Component', () => {
  it('obterInfoClassificacao deve retornar Livre para 0 ou valores nulos', () => {
    expect(obterInfoClassificacao(0).codigo).toBe('L');
    expect(obterInfoClassificacao(null).codigo).toBe('L');
    expect(obterInfoClassificacao('Livre').codigo).toBe('L');
  });

  it('obterInfoClassificacao deve mapear faixas etárias corretamente', () => {
    expect(obterInfoClassificacao(10).codigo).toBe('10');
    expect(obterInfoClassificacao(12).codigo).toBe('12');
    expect(obterInfoClassificacao(14).codigo).toBe('14');
    expect(obterInfoClassificacao(16).codigo).toBe('16');
    expect(obterInfoClassificacao(18).codigo).toBe('18');
  });

  it('deve renderizar o badge com a classe e código corretos', () => {
    render(<ClassificacaoBadge classificacao={18} />);
    const badge = screen.getByText('18');
    expect(badge).toBeInTheDocument();
    expect(badge).toHaveClass('classind-18');
  });

  it('deve renderizar o rótulo descritivo quando showLabel é true', () => {
    render(<ClassificacaoBadge classificacao={16} showLabel={true} />);
    expect(screen.getByText('16')).toBeInTheDocument();
    expect(screen.getByText('16 Anos')).toBeInTheDocument();
  });
});
