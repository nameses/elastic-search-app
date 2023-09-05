import { Injectable } from '@angular/core';
import { Book } from 'src/models/Book';

@Injectable({
  providedIn: 'root',
})
export class SearchPageService {
  dataList: Book[] = [];
  constructor() {}

  public LoadBooks(books: Book[]): void {
    this.dataList = books;
    console.log(this.dataList);
    console.log(books);
  }
}
