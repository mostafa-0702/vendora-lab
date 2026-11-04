import { useEffect, useState } from 'react'

type Product = {
  id: number
  name: string
  price: number
  createdAt: string
}

type ApiInfo = {
  version: string
  environment: string
}

const API_URL = import.meta.env.VITE_API_URL
const ENV_NAME: string = import.meta.env.VITE_ENV_NAME ?? 'inconnu'
const IS_PROD = ENV_NAME === 'production'

const bannerColors: Record<string, string> = {
  development: '#3b82f6', // bleu
  staging: '#f59e0b',     // orange
}

function App() {
  const [products, setProducts] = useState<Product[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [apiInfo, setApiInfo] = useState<ApiInfo | null>(null)

  useEffect(() => {
    // 1. Les produits
    fetch(`${API_URL}/api/products`)
      .then((res) => {
        if (!res.ok) throw new Error(`HTTP ${res.status}`)
        return res.json()
      })
      .then((data: Product[]) => setProducts(data))
      .catch((err: Error) => setError(err.message))
      .finally(() => setLoading(false))

    // 2. Qui est l'API que j'appelle vraiment ?
    fetch(`${API_URL}/api/info`)
      .then((res) => (res.ok ? res.json() : null))
      .then((data: ApiInfo | null) => setApiInfo(data))
      .catch(() => setApiInfo(null))
  }, [])

  return (
    <>
      {!IS_PROD && (
        <div
          style={{
            background: bannerColors[ENV_NAME] ?? '#ef4444',
            color: '#000',
            fontWeight: 'bold',
            textAlign: 'center',
            padding: '8px',
          }}
        >
          ENVIRONNEMENT : {ENV_NAME.toUpperCase()}
        </div>
      )}

      <main>
        <h1>Vendora</h1>

        {loading && <p>Chargement…</p>}
        {error && <p>Impossible de charger les produits : {error}</p>}

        {!loading && !error && (
          <>
            <h2>Produits ({products.length})</h2>
            <table>
              <thead>
                <tr>
                  <th>#</th>
                  <th>Nom</th>
                  <th>Prix</th>
                </tr>
              </thead>
              <tbody>
                {products.map((p) => (
                  <tr key={p.id}>
                    <td>{p.id}</td>
                    <td>{p.name}</td>
                    <td>{p.price.toFixed(2)} MAD</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </>
        )}
      </main>

      <footer style={{ marginTop: '32px', fontSize: '12px', color: '#666' }}>
        Front : {ENV_NAME} · API :{' '}
        {apiInfo ? `${apiInfo.environment} (v${apiInfo.version})` : 'indisponible'}
      </footer>
    </>
  )
}

export default App