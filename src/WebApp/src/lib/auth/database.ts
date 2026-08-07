import 'dotenv/config'
import { drizzle } from 'drizzle-orm/node-postgres'

export const createDbUrl = () => {
  const {
    POSTGRES_USER,
    POSTGRES_PASSWORD,
    POSTGRES_HOST,
    POSTGRES_PORT,
    POSTGRES_DATABASE,
    POSTGRES_URI
  } = process.env

  if (POSTGRES_URI !== undefined && POSTGRES_URI !== "") {
    console.log(`using POSTGRES_URI ${POSTGRES_URI}`)
    return POSTGRES_URI
  }
  console.log(`Using POSTGRES variables`)
  return `postgresql://${POSTGRES_USER}:${POSTGRES_PASSWORD}@${POSTGRES_HOST}:${POSTGRES_PORT}/${POSTGRES_DATABASE}`
}

export const db = drizzle(createDbUrl())
