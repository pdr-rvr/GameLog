import React from "react";
import "./ClassificacaoBadge.css";

const CLASSIFICACOES = {
  0: {
    codigo: "L",
    nome: "Livre",
    descricao: "Classificação Livre para todos os públicos",
    classe: "classind-l"
  },
  10: {
    codigo: "10",
    nome: "10 Anos",
    descricao: "Não recomendado para menores de 10 anos",
    classe: "classind-10"
  },
  12: {
    codigo: "12",
    nome: "12 Anos",
    descricao: "Não recomendado para menores de 12 anos",
    classe: "classind-12"
  },
  14: {
    codigo: "14",
    nome: "14 Anos",
    descricao: "Não recomendado para menores de 14 anos",
    classe: "classind-14"
  },
  16: {
    codigo: "16",
    nome: "16 Anos",
    descricao: "Não recomendado para menores de 16 anos",
    classe: "classind-16"
  },
  18: {
    codigo: "18",
    nome: "18 Anos",
    descricao: "Não recomendado para menores de 18 anos",
    classe: "classind-18"
  }
};

export const obterInfoClassificacao = (valor) => {
  if (valor === null || valor === undefined || valor === "" || valor === "L" || valor === "Livre") {
    return CLASSIFICACOES[0];
  }
  const num = parseInt(valor, 10);
  if (isNaN(num) || num <= 0) return CLASSIFICACOES[0];
  if (num < 10) return CLASSIFICACOES[0];
  if (num < 12) return CLASSIFICACOES[10];
  if (num < 14) return CLASSIFICACOES[12];
  if (num < 16) return CLASSIFICACOES[14];
  if (num < 18) return CLASSIFICACOES[16];
  return CLASSIFICACOES[18];
};

const ClassificacaoBadge = ({ classificacao, size = "md", showLabel = false, className = "" }) => {
  const info = obterInfoClassificacao(classificacao);

  return (
    <div
      className={`classind-container size-${size} ${className}`}
      title={info.descricao}
    >
      <span className={`classind-badge ${info.classe} size-${size}`}>
        {info.codigo}
      </span>
      {showLabel && (
        <span className="classind-label-text">
          {info.nome}
        </span>
      )}
    </div>
  );
};

export default ClassificacaoBadge;
