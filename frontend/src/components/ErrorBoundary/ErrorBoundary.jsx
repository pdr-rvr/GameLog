import React from 'react';
import './ErrorBoundary.css';

export class ErrorBoundary extends React.Component {
  constructor(props) {
    super(props);
    this.state = { hasError: false, error: null };
  }

  static getDerivedStateFromError(error) {
    return { hasError: true, error };
  }

  componentDidCatch(error, errorInfo) {
    if (process.env.NODE_ENV !== 'production') {
      console.error('[ErrorBoundary caught error]:', error, errorInfo);
    }
  }

  handleReload = () => {
    window.location.reload();
  };

  handleReset = () => {
    this.setState({ hasError: false, error: null });
    if (this.props.onReset) {
      this.props.onReset();
    }
  };

  render() {
    if (this.state.hasError) {
      if (this.props.fallback) {
        return this.props.fallback;
      }

      return (
        <div className="error-boundary-container">
          <div className="error-boundary-card">
            <div className="error-boundary-icon">⚠️</div>
            <h2 className="error-boundary-title">Ops! Algo deu errado</h2>
            <p className="error-boundary-message">
              Ocorreu um problema inesperado ao renderizar esta seção. Tente recarregar a página.
            </p>
            <div className="error-boundary-actions">
              <button 
                type="button" 
                className="error-boundary-btn error-boundary-btn-primary"
                onClick={this.handleReload}
              >
                Recarregar Página
              </button>
              <button 
                type="button" 
                className="error-boundary-btn error-boundary-btn-secondary"
                onClick={this.handleReset}
              >
                Tentar Novamente
              </button>
            </div>
          </div>
        </div>
      );
    }

    return this.props.children;
  }
}

export default ErrorBoundary;
