import React from "react";
import { Link } from "react-router-dom";
import "./JogoCard.css";

const JogoCard = ({ jogo }) => {
    const anoLancamento = jogo.dataLancamento ? parseInt(jogo.dataLancamento.toString().substring(0, 4)) : "N/A";

    return (
        <Link to={`/jogos/${jogo.jogoId}`} className="jogo-card-link">
            <div className="jogo-card">
                <div className="jogo-card-image-container">
                    <img 
                        src={jogo.imagem || "/game-images/default_game_cover.png"}
                        alt={jogo.titulo} 
                        className="jogo-card-image"
                        onError={(e) => {
                            e.target.onerror = null;
                            e.target.src = "/game-images/default_game_cover.png";
                        }}
                    />
                </div>
                <div className="jogo-card-details">
                    <h3 className="jogo-card-title">{jogo.titulo}</h3>
                    <p className="jogo-card-year">{anoLancamento}</p>
                    {jogo.mediaAvaliacoes !== null && jogo.mediaAvaliacoes !== undefined && (
                        <div className="jogo-card-rating">
                            ⭐ {jogo.mediaAvaliacoes.toFixed(1)}
                        </div>
                    )}
                </div>
            </div>
        </Link>
    );
};

export default JogoCard;
