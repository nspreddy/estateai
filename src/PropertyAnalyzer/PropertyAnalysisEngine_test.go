package PropertyAnalyzer

import (
	"fmt"
	"testing"
	"time"
)

func TestTimeFormat(t *testing.T) {
	currTime := time.Now()
	timeStr := fmt.Sprintf("%04d%02d%02d%02d", currTime.Year(), currTime.Month(), currTime.Day(), currTime.Hour())
	fmt.Println(timeStr)
}
