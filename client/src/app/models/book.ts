import { Author } from './author';

export interface Book {
  id?: string;
  title: string;
  isbn: string;
  authorIds?: Array<string>;
  authors?: Array<Author>;
  hasFile?: boolean;
}
