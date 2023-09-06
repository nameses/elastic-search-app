import { HttpClient, HttpParams } from '@angular/common/http';
import { ChangeDetectorRef, Component } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { Book } from 'src/models/Book';
import { SearchService } from '../services/search.service';
import { BehaviorSubject, Subject, catchError, of, timeout } from 'rxjs';
import { PagedList } from 'src/types/pagedList';

@Component({
  selector: 'app-search',
  templateUrl: './search.component.html',
  styleUrls: [],
})
export class SearchComponent {
  public dataList: PagedList<Book> | undefined;
  private pageSize: number = 12;
  currentPage$ = new BehaviorSubject<number>(1);

  searchForm: FormGroup = this.formBuilder.group({
    query: '',
  });

  constructor(
    private formBuilder: FormBuilder,
    public searchService: SearchService,
    private cdRef: ChangeDetectorRef
  ) {}

  onSubmit(): void {
    if (this.searchForm.get('query')?.value.trim() == '') {
      console.warn('Empty query');
      return;
    }
    if (this.searchForm.get('query')?.value.trim().length < 4) {
      console.warn('Too small query');
      return;
    }
    this.currentPage$.next(1);
    this.renewData();
  }

  public nextPage(): void {
    if (!this.dataList?.hasNextPage) {
      console.warn('No next page');
      return;
    }
    if (this.dataList?.page == this.dataList?.totalPageCount) return;
    this.currentPage$.next(this.currentPage$.value + 1);
    this.renewData();
  }

  public previousPage(): void {
    if (!this.dataList?.hasPreviousPage) {
      console.warn('No previous page');
      return;
    }
    this.currentPage$.next(this.currentPage$.value - 1);
    this.renewData();
  }

  renewData(): void {
    this.searchService
      .search(
        this.searchForm.get('query')?.value.trim(),
        this.currentPage$.value,
        this.pageSize
      )
      .subscribe((response) => {
        this.dataList = response;
        this.dataList!.totalPageCount = Math.ceil(
          response.totalCount / response.pageSize
        );
        console.log(this.dataList);

        this.cdRef.detectChanges();
      });
  }
}
