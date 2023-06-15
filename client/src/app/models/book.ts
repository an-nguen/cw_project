import { Author } from './author'

export interface Book {
  id?: string
  title: string
  isbn: string
  authors: Array<Author> | Array<string>
}
