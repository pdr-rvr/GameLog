import './Background.css';
import backgroundSvg from './FundoLoginCadastro.svg';

function Background() {
  return (
    <div className="background-container">
      <img src={backgroundSvg} alt="Background" className="background-svg" />
    </div>
  );
}

export default Background;
