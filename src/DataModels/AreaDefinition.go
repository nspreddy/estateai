package DataModels

type AreaDefinition struct {
	IsLocationLatLong bool    `json:"IsLatLong"`
	Lat               float64 `json:"Lat"`
	Lng               float64 `json:"Long"`
	Address           string  `json:"Address"`
	Radius            rune    `json:"Radius"`
	Name              string  `json:"Name"`
}
