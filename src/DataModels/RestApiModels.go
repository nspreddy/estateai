package DataModels

type MockApiModel struct {
	Name      string
	LiveCheck bool
}

type HealthCheck struct {
	LiveCheck    bool
	ErrorMessage error
}
