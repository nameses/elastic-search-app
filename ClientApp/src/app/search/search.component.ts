import { HttpClient, HttpParams } from '@angular/common/http';
import { ChangeDetectorRef, Component } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { Book } from 'src/models/Book';
import { SearchPageService } from '../services/search-page.service';
import { Subject } from 'rxjs';

@Component({
  selector: 'app-search',
  templateUrl: './search.component.html',
  styleUrls: [],
})
export class SearchComponent {
  constructor(
    private formBuilder: FormBuilder,
    private http: HttpClient,
    public searchService: SearchPageService,
    private cdRef: ChangeDetectorRef
  ) {
    // this.dataList.next([]);
  }

  public dataList: Book[] = [];
  // dataList: Subject<Book[]> = new Subject<Book[]>();

  searchForm: FormGroup = this.formBuilder.group({
    query: '',
  });

  onSubmit(): void {
    // if (this.searchForm.get('query')?.value) {
    //   console.log('Query is empty');
    //   return;
    // }

    const apiUrl = 'https://localhost:7037/api/book/search';
    const params = new HttpParams().set(
      'query',
      this.searchForm.get('query')?.value
    );

    console.log('Your search query has been submitted');

    this.http.get<Book[]>(apiUrl, { params }).subscribe((response) => {
      this.dataList = response;
      // this.dataList.next(response);
      console.log('1: ', this.dataList);
      console.log('2: ', response);

      this.cdRef.detectChanges();
    });
  }
}
