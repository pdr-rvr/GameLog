
import './FundoFormCadastro.svg';
import fundoFormSvg from './FundoFormCadastro.svg';

function FundoForm() {
  return (
    <div className="fundo-form-container">
      <img
        src={fundoFormSvg}
        alt="Fundo do Formulário"
        className="fundo-form-svg"
      />
    </div>
  );
}

export default FundoForm;
