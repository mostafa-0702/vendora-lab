import { useEffect, useState } from 'react'
import './App.css'

// La "forme" d'un produit, la même que dans l'API (record Product)
type Product = { id: number; name: string; price: number; createdAt: string }

// L'adresse de l'API, lue dans .env.development
const API_URL = import.meta.env.VITE_API_URL

function App() {
  const [products, setProducts] = useState<Product[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  // Au chargement de la page : appeler l'API une fois
  useEffect(() => {
    fetch(`${API_URL}/api/products`)
      .then((res) => {
        if (!res.ok) throw new Error(`Erreur HTTP ${res.status}`)
        return res.json()
      })
      .then((data: Product[]) => setProducts(data))
      .catch((err: Error) => setError(err.message))
      .finally(() => setLoading(false))
  }, [])

  if (loading) return <p>Chargement…</p>
  if (error) return <p style={{ color: 'red' }}>Impossible de charger les produits : {error}</p>

  return (
    <main>
      <h1>Vendora</h1>
      <h2>Produits ({products.length})</h2>
      <table>
        <thead>
          <tr><th>#</th><th>Nom</th><th>Prix</th></tr>
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
    </main>
  )
}

export default App