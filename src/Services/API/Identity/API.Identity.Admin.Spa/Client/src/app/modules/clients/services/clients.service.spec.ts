import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { ClientsService } from './clients.service';
import { ConfigService } from '@app/core/services/config.service';
import { Client } from '../models/client.model';

describe('ClientsService', () => {
  let service: ClientsService;
  let httpMock: HttpTestingController;
  let configService: jasmine.SpyObj<ConfigService>;

  beforeEach(() => {
    const configServiceSpy = jasmine.createSpyObj('ConfigService', [], {
      hbgidentityadminapi: 'https://test-api.local'
    });

    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        ClientsService,
        { provide: ConfigService, useValue: configServiceSpy }
      ]
    });

    service = TestBed.inject(ClientsService);
    httpMock = TestBed.inject(HttpTestingController);
    configService = TestBed.inject(ConfigService) as jasmine.SpyObj<ConfigService>;
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should have dataSource configured with correct API endpoint', () => {
    expect(service.dataSource).toBeDefined();
    expect(service['dataStore']).toBeDefined();
  });

  it('should create RestDataStore with correct URLs', () => {
    const expectedBaseUrl = 'https://test-api.local/api/clients';
    expect(service['dataStore']['urls'].loadUrl).toBe(expectedBaseUrl);
    expect(service['dataStore']['urls'].insertUrl).toBe(expectedBaseUrl);
    expect(service['dataStore']['urls'].removeUrl).toBe(expectedBaseUrl);
  });

  it('should have CustomDataSource with RestDataStore', () => {
    expect(service.dataSource.customStore).toBe(service['dataStore']);
  });
});
