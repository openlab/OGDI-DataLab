Command line:

DataLoaderUtility.exe /type=dbf+kml|csv /fsname=file_set_name /target=console|tables [/date] [/mode=create|add|update] [/sourceorder].

Note:
1) File set (.csv and .cfg files) must be located in the current directory.
2) If you choose to preserve original sort order by using /sourceorder, switch, you will not be able to use /mode=add|update in future.
3) It is assumed that all date/time values in the source (.csv) data file belong to the same time zone. That time zone value should be specified in the corresponding configuration (.cfg) file. If time zone value is not specified, current time zone is assumed. To specify the time zone add SourceTimeZoneName child element under TableMetadataEntity element:
      
      <CsvDataLoaderParams xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
        <TableMetadataEntity>     ...           </TableMetadataEntity>
        <ProducerParams>          ...           </ProducerParams>
        <ProcessorParams>

          <SourceTimeZoneName>(GMT-05:00) Eastern Time (US &amp; Canada)</SourceTimeZoneName>
          ...
        </ProcessorParams>
      </CsvDataLoaderParams>
      
Valid time zone values are listed below:      

(GMT-12:00) International Date Line West
(GMT-11:00) Midway Island, Samoa
(GMT-10:00) Hawaii
(GMT-09:00) Alaska
(GMT-08:00) Pacific Time (US &amp; Canada)
(GMT-08:00) Tijuana, Baja California
(GMT-07:00) Arizona
(GMT-07:00) Chihuahua, La Paz, Mazatlan
(GMT-07:00) Mountain Time (US &amp; Canada)
(GMT-06:00) Central Time (US &amp; Canada)
(GMT-06:00) Central America
(GMT-06:00) Saskatchewan
(GMT-06:00) Guadalajara, Mexico City, Monterrey
(GMT-05:00) Bogota, Lima, Quito
(GMT-05:00) Eastern Time (US &amp; Canada)
(GMT-05:00) Indiana (East)
(GMT-04:00) Georgetown, La Paz, San Juan
(GMT-04:00) Atlantic Time (Canada)
(GMT-04:00) Asuncion
(GMT-04:30) Caracas
(GMT-04:00) Santiago
(GMT-04:00) Manaus
(GMT-03:00) Cayenne
(GMT-03:00) Buenos Aires
(GMT-03:00) Brasilia
(GMT-03:30) Newfoundland
(GMT-03:00) Montevideo
(GMT-03:00) Greenland
(GMT-02:00) Mid-Atlantic
(GMT-01:00) Azores
(GMT-01:00) Cape Verde Is.
(GMT) Casablanca
(GMT) Monrovia, Reykjavik
(GMT) Greenwich Mean Time : Dublin, Edinburgh, Lisbon, London
(GMT) Coordinated Universal Time
(GMT+01:00) Sarajevo, Skopje, Warsaw, Zagreb
(GMT+01:00) West Central Africa
(GMT+01:00) Brussels, Copenhagen, Madrid, Paris
(GMT+01:00) Amsterdam, Berlin, Bern, Rome, Stockholm, Vienna
(GMT+01:00) Belgrade, Bratislava, Budapest, Ljubljana, Prague
(GMT+02:00) Jerusalem
(GMT+02:00) Helsinki, Kyiv, Riga, Sofia, Tallinn, Vilnius
(GMT+02:00) Windhoek
(GMT+02:00) Minsk
(GMT+02:00) Harare, Pretoria
(GMT+02:00) Athens, Bucharest, Istanbul
(GMT+02:00) Amman
(GMT+02:00) Cairo
(GMT+02:00) Beirut
(GMT+03:00) Nairobi
(GMT+03:30) Tehran
(GMT+03:00) Moscow, St. Petersburg, Volgograd
(GMT+03:00) Baghdad
(GMT+03:00) Kuwait, Riyadh
(GMT+04:00) Tbilisi
(GMT+04:00) Yerevan
(GMT+04:30) Kabul
(GMT+04:00) Abu Dhabi, Muscat
(GMT+04:00) Baku
(GMT+04:00) Port Louis
(GMT+05:00) Ekaterinburg
(GMT+05:00) Islamabad, Karachi
(GMT+05:30) Sri Jayawardenepura
(GMT+05:45) Kathmandu
(GMT+05:00) Tashkent
(GMT+05:30) Chennai, Kolkata, Mumbai, New Delhi
(GMT+06:30) Yangon (Rangoon)
(GMT+06:00) Novosibirsk
(GMT+06:00) Astana, Dhaka
(GMT+07:00) Krasnoyarsk
(GMT+07:00) Bangkok, Hanoi, Jakarta
(GMT+08:00) Perth
(GMT+08:00) Taipei
(GMT+08:00) Ulaanbaatar
(GMT+08:00) Beijing, Chongqing, Hong Kong, Urumqi
(GMT+08:00) Irkutsk
(GMT+08:00) Kuala Lumpur, Singapore
(GMT+09:30) Adelaidea
(GMT+09:30) Darwin
(GMT+09:00) Yakutsk
(GMT+09:00) Osaka, Sapporo, Tokyo
(GMT+09:00) Seoul
(GMT+10:00) Hobart
(GMT+10:00) Vladivostok
(GMT+10:00) Guam, Port Moresby
(GMT+10:00) Brisbane
(GMT+10:00) Canberra, Melbourne, Sydney
(GMT+11:00) Magadan, Solomon Is., New Caledonia
(GMT+12:00) Petropavlovsk-Kamchatsky
(GMT+12:00) Fiji, Marshall Is.
(GMT+12:00) Auckland, Wellington
(GMT+13:00) Nuku'alofa

